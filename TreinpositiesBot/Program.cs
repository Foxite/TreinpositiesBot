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
using TreinpositiesBot.Config;
using TreinpositiesBot.SourceConfig;

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
		var sourcesConfig = hbc.Configuration.GetSection("Sources");
		var boundSourcesConfig = sourcesConfig.Get<SourcesConfig>()!;
		isc.Configure<SourcesConfig>(sourcesConfig);
		isc.Configure<TreinpositiesPhotoSource.Options>(hbc.Configuration.GetSection("Treinposities"));

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

		isc.AddScoped<PhotoSourceService>();

		isc.Add(new ServiceDescriptor(typeof(PhotoSourceProvider), boundSourcesConfig.Source switch {
			SourcesConfigSource.Config => typeof(ConfigPhotoSourceProvider),
			SourcesConfigSource.Postgres => typeof(TreinpositiesBot.SourceConfig.Database.DatabasePhotoSourceProvider),
		}, ServiceLifetime.Scoped));
	})
	.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var coreConfig = host.Services.GetRequiredService<IOptionsMonitor<CoreConfig>>();
var discord = host.Services.GetRequiredService<DiscordClient>();

var lastSendPerUser = new ConcurrentDictionary<ulong, DateTime>();

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

discord.MessageCreated += (unused, args) => {
	if (args.Guild != null) {
		Permissions perms = args.Channel.PermissionsFor(args.Guild.CurrentMember);
		if (!perms.HasFlag(args.Channel.IsThread ? Permissions.SendMessagesInThreads : Permissions.SendMessages)) {
			return Task.CompletedTask;
		}
	}

	if (args.Author.IsBot) {
		return Task.CompletedTask;
	}

	_ = Task.Run(async () => {
		try {
			await using var scope = host.Services.CreateAsyncScope();
			List<PhotoSource> sources = await scope.ServiceProvider.GetRequiredService<PhotoSourceService>().GetPhotoSourcesAsync(args.Channel);
			sources.Shuffle();

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

			if (lastSendPerUser.TryGetValue(args.Author.Id, out DateTime lastSend) && DateTime.UtcNow - lastSend < coreConfig.CurrentValue.Cooldown) {
				try {
					await args.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("⏲️"));
				} catch (NotFoundException) {
				} catch (UnauthorizedException) {
				}
				return;
			}
			
			Photobox? photobox = await chosenSource.GetPhoto(ids);
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
			
			// This reaction is to test if the user has blocked the bot.
			// If they have, then we don't reply.
			try {
				await args.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("📷"));
			} catch (NotFoundException) {
				// Message was deleted
				return;
			} catch (UnauthorizedException) {
				// User blocked bot - don't reply
				return;
			}
			
			// User did not block bot

			string typeName = photobox.PhotoType switch {
				PhotoType.General => "Foto",
				PhotoType.Interior => "Interieurfoto",
				PhotoType.Detail => "Detailfoto",
				PhotoType.Cabin => "Cabinefoto",
				PhotoType.EngineRoom => "Motorruimtefoto"
			};

			lastSendPerUser[args.Message.Author.Id] = DateTime.UtcNow;
			try {
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
			}
		} catch (Exception ex) {
			FormattableString report = $"Error responding to message {args.Message.Id} ({args.Message.JumpLink})";
					
			logger.LogCritical(ex, report);
					
			if (notifications != null) {
				await notifications.SendNotificationAsync(report.ToString(), ex.Demystify());
			}
		}
	});

	return Task.CompletedTask;
};

await discord.ConnectAsync();
await Task.Delay(-1);
