using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Foxite.Common;
using Foxite.Common.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TreinpositiesBot;

var host = Host.CreateDefaultBuilder()
	.ConfigureAppConfiguration((hbc, icb) => {
		icb.AddJsonFile("appsettings.json");
		icb.AddJsonFile($"appsettings.{hbc.HostingEnvironment.EnvironmentName}.json", true);
		icb.AddEnvironmentVariables();
		icb.AddCommandLine(args);
	})
	.ConfigureLogging((hbc, ilb) => {
		//ilb.AddSimpleConsole();
	})
	.ConfigureServices((hbc, isc) => {
		isc.Configure<CoreConfig>(hbc.Configuration.GetSection("Core"));
		isc.Configure<TreinpositiesPhotoSource.Options>(hbc.Configuration.GetSection("Treinposities"));
		isc.Configure<JetphotosPhotoSource.Options>(hbc.Configuration.GetSection("Jetphotos"));

		isc.AddSingleton<Random>();	
		
		isc.AddSingleton(isp => new DiscordClient(new DiscordConfiguration() {
			Token = isp.GetRequiredService<IOptions<CoreConfig>>().Value.DiscordToken,
			Intents = DiscordIntents.GuildMessages | DiscordIntents.Guilds,
			LoggerFactory = isp.GetRequiredService<ILoggerFactory>(),
		}));

		isc.AddSingleton(_ => new HttpClientHandler() {
			AllowAutoRedirect = false,
		});

		isc.AddSingleton(isp => {
			var ret = new HttpClient(isp.GetRequiredService<HttpClientHandler>());

			ret.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TreinpositiesBot", "0.4"));
			ret.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Foxite/TreinpositiesBot)"));

			return ret;
		});
		
		isc.TryAddEnumerable(ServiceDescriptor.Singleton<PhotoSource, TreinBusPositiesPhotoSource>());
		isc.TryAddEnumerable(ServiceDescriptor.Singleton<PhotoSource, BuspositiesPhotoSource>());
		isc.TryAddEnumerable(ServiceDescriptor.Singleton<PhotoSource, TreinpositiesPhotoSource>());
		isc.TryAddEnumerable(ServiceDescriptor.Singleton<PhotoSource, PlanespottersScrapingPhotoSource>());
		isc.TryAddEnumerable(ServiceDescriptor.Singleton<PhotoSource, JetphotosPhotoSource>());

		isc.AddSingleton<PhotoSourceProvider>();

		isc.AddSingleton<ChannelConfigService, ConfigChannelConfigService>();
	})
	.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var coreConfig = host.Services.GetRequiredService<IOptionsMonitor<CoreConfig>>();
var discord = host.Services.GetRequiredService<DiscordClient>();

var cooldowns = new ConcurrentDictionary<(ulong UserId, ulong ChannelId), DateTime>();

DiscordWebhookLibNotificationSender? notifications = null;
string? webhookUrl = coreConfig.CurrentValue.ErrorWebhookUrl;
if (!string.IsNullOrWhiteSpace(webhookUrl)) {
	notifications = new DiscordWebhookLibNotificationSender(new OptionsWrapper<DiscordWebhookLibNotificationSender.Config>(new DiscordWebhookLibNotificationSender.Config() {
		WebhookUrl = webhookUrl
	}));
}

discord.ClientErrored += (_, args) => {
	logger.LogError(args.Exception.Demystify(), "Discord client error");
	return Task.CompletedTask;
};

discord.MessageCreated += async (unused, args) => {
	if (args.Guild != null) {
		Permissions perms = args.Channel.PermissionsFor(args.Guild.CurrentMember);
		if (!perms.HasFlag(args.Channel.IsThread ? Permissions.SendMessagesInThreads : Permissions.SendMessages)) {
			return;
		}
	}

	if (args.Author.IsBot) {
		return;
	}

	var ccs = host.Services.GetRequiredService<ChannelConfigService>();
	var sources = await host.Services.GetRequiredService<PhotoSourceProvider>().GetPhotoSources(args.Channel);

	PhotoSource? chosenSource = null;
	IReadOnlyCollection<string> ids = Array.Empty<string>();

	foreach (PhotoSource source in sources) {
		ids = source.ExtractIds(args.Message.Content);
		if (ids.Count > 0) {
			chosenSource = source;
			break;
		}
	}

	if (chosenSource == null) {
		return;
	}
	
	if (cooldowns.TryGetValue((args.Author.Id, args.Channel.Id), out DateTime lastSend) && DateTime.UtcNow - lastSend <= await ccs.GetCooldownAsync(args.Channel)) {
		try {
			await args.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("⏲️"));
		} catch (NotFoundException) {
			// Message was deleted
		} catch (UnauthorizedException) {
			// User blocked bot
		}
		return;
	}

	_ = Task.Run(async () => {
		Photobox? photobox = null;
		try {
			photobox = await chosenSource.GetPhoto(ids);
			if (photobox == null) {
				logger.LogInformation("No matches found for {Ids}", string.Join(", ", ids));
				
				if (string.IsNullOrWhiteSpace(coreConfig.CurrentValue.NoResultsEmote)) {
					return;
				}

				try {
					await args.Message.CreateReactionAsync(DiscordEmoji.FromName(discord, coreConfig.CurrentValue.NoResultsEmote, true));
				} catch (NotFoundException) {
				} catch (UnauthorizedException) {
				}
				
				return;
			}

			try {
				await args.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("📷"));
			} catch (NotFoundException) {
				return;
			} catch (UnauthorizedException) {
				return;
			}

			string typeName = photobox.PhotoType switch {
				PhotoType.General => "Foto",
				PhotoType.Interior => "Interieurfoto",
				PhotoType.Detail => "Detailfoto",
				PhotoType.Cabin => "Cabinefoto",
				PhotoType.EngineRoom => "Motorruimtefoto"
			};

			cooldowns[(args.Message.Author.Id, args.Channel.Id)] = DateTime.UtcNow;
			try {
				logger.LogDebug("Sending photobox\nAuthor: {Author} {AuthorUrl}\nUrl: {Url}\nImage url: {ImageUrl}", photobox.Photographer, photobox.PhotographerUrl, photobox.PageUrl, photobox.ImageUrl);
				await args.Message.RespondAsync(dmb => dmb
					.WithEmbed(new DiscordEmbedBuilder()
						.WithAuthor(photobox.Photographer, photobox.PhotographerUrl)
						.WithTitle($"{typeName} van {photobox.Identity}")
						.WithUrl(photobox.PageUrl)
						.WithImageUrl(photobox.ImageUrl)
						.WithFooter($"© {photobox.Photographer}, {photobox.Taken} | Geen reacties meer? Blokkeer mij")
					)
				);
			} catch (NotFoundException) {
				// Message was deleted
			}
		} catch (Exception ex) {
			FormattableString report = $"Error responding to message {args.Message.Id} ({args.Message.JumpLink}), numbers: {string.Join(", ", ids)}; photo url: {photobox?.PageUrl ?? "null"}";

			logger.LogCritical(ex, report);

			if (notifications != null) {
				await notifications.SendNotificationAsync(report.ToString(), ex.Demystify());
			}
		}
	});
};

await discord.ConnectAsync();
await Task.Delay(-1);
