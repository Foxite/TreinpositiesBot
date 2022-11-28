using System.Collections.Concurrent;
using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Foxite.Common.Notifications;
using Microsoft.Extensions.Options;
using TreinpositiesBot;

var discord = new DiscordClient(new DiscordConfiguration() {
	Token = Environment.GetEnvironmentVariable("BOT_TOKEN"),
	Intents = DiscordIntents.GuildMessages | DiscordIntents.Guilds
});

discord.ClientErrored += (_, args) => {
	Console.WriteLine(args.Exception.ToStringDemystified());
	return Task.CompletedTask;
};

DiscordWebhookLibNotificationSender? notifications = null;
string? webhookUrl = Environment.GetEnvironmentVariable("WEBHOOK_URL");
if (webhookUrl != null) {
	notifications = new DiscordWebhookLibNotificationSender(new OptionsWrapper<DiscordWebhookLibNotificationSender.Config>(new DiscordWebhookLibNotificationSender.Config() {
		WebhookUrl = webhookUrl
	}));
}

var lastSendPerUser = new ConcurrentDictionary<ulong, DateTime>();
string? cooldownEnvvar = Environment.GetEnvironmentVariable("COOLDOWN_SECONDS");
TimeSpan cooldown = cooldownEnvvar == null ? TimeSpan.FromSeconds(60) : TimeSpan.FromSeconds(int.Parse(cooldownEnvvar));

PhotoSource photoSource = new TreinpositiesPhotoSource();

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

	IReadOnlyCollection<string> ids = photoSource.ExtractIds(args.Message.Content);
	if (ids.Count > 0) {
		if (!lastSendPerUser.TryGetValue(args.Author.Id, out DateTime lastSend) || DateTime.UtcNow - lastSend > cooldown) {
			_ = Task.Run(async () => {
				Photobox? photobox = null;
				try {
					photobox = await photoSource.GetPhoto(ids);
					if (photobox == null) {
						string? noPhotosReactionEnvvar = Environment.GetEnvironmentVariable("NO_RESULTS_EMOTE");
						if (noPhotosReactionEnvvar != null) {
							try {
								await args.Message.CreateReactionAsync(DiscordEmoji.FromName(discord, noPhotosReactionEnvvar, true));
							} catch (UnauthorizedException) {
							}
						}
					} else {
						try {
							await args.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("📷"));
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

						lastSendPerUser[args.Message.Author.Id] = DateTime.UtcNow;
						await args.Message.RespondAsync(dmb => dmb
							.WithEmbed(new DiscordEmbedBuilder()
								.WithAuthor(photobox.Photographer, photobox.PhotographerUrl)
								.WithTitle($"{typeName} van {photobox.Identity}")
								.WithUrl(photobox.PageUrl)
								.WithImageUrl(photobox.ImageUrl)
								.WithFooter($"© {photobox.Photographer}, {photobox.Taken} | Geen reacties meer? Blokkeer mij")
							)
						);
					}
				} catch (Exception e) {
					string report = $"Error responding to message {args.Message.Id} ({args.Message.JumpLink}), numbers: {string.Join(", ", ids)}; photo url: ${(photobox?.PageUrl ?? "null")}";
					Console.WriteLine(report);
					Console.WriteLine(e.ToStringDemystified());
					if (notifications != null) {
						await notifications.SendNotificationAsync(report, e.Demystify());
					}
				}
			});
		} else {
			try {
				return args.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("⏲️"));
			} catch (UnauthorizedException) { }
		}
	}

	return Task.CompletedTask;
};

await discord.ConnectAsync();
await Task.Delay(-1);
