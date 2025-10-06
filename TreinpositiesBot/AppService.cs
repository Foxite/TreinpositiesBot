using System.Collections.Concurrent;
using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Foxite.Common;
using Foxite.Common.Notifications;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TreinpositiesBot;

public class AppService : IHostedService {
	private readonly ILogger<AppService> m_Logger;
	private readonly IOptionsMonitor<CoreConfig> m_CoreConfig;
	private readonly DiscordClient m_Discord;
	private readonly ChannelConfigService m_ChannelConfigService;
	private readonly PhotoSourceProvider m_PhotoSourceProvider;
	private readonly ConcurrentDictionary<(ulong UserId, ulong ChannelId), DateTime> m_Cooldowns;
	private readonly DiscordWebhookLibNotificationSender? m_Notifications;

	public AppService(ILogger<AppService> logger, IOptionsMonitor<CoreConfig> coreConfig, DiscordClient discord, ChannelConfigService channelConfigService, PhotoSourceProvider photoSourceProvider) {
		m_Logger = logger;
		m_CoreConfig = coreConfig;
		m_Discord = discord;
		m_ChannelConfigService = channelConfigService;
		m_PhotoSourceProvider = photoSourceProvider;
		m_Cooldowns = new ConcurrentDictionary<(ulong UserId, ulong ChannelId), DateTime>();

		string? webhookUrl = coreConfig.CurrentValue.ErrorWebhookUrl;
		if (!string.IsNullOrWhiteSpace(webhookUrl)) {
			m_Notifications = new DiscordWebhookLibNotificationSender(new OptionsWrapper<DiscordWebhookLibNotificationSender.Config>(new DiscordWebhookLibNotificationSender.Config() {
				WebhookUrl = webhookUrl,
			}));
		}
	}
	
	public Task StartAsync(CancellationToken cancellationToken) {
		m_Discord.MessageCreated += OnDiscordOnMessageCreated;
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken) {
		m_Discord.MessageCreated -= OnDiscordOnMessageCreated;
		return Task.CompletedTask;
	}


	private async Task OnDiscordOnMessageCreated(DiscordClient unused, MessageCreateEventArgs args) {
		if (args.Guild != null) {
			Permissions perms = args.Channel.PermissionsFor(args.Guild.CurrentMember);
			if (!perms.HasFlag(args.Channel.IsThread ? Permissions.SendMessagesInThreads : Permissions.SendMessages)) {
				return;
			}
		}

		if (args.Author.IsBot) {
			return;
		}

		var sources = await m_PhotoSourceProvider.GetPhotoSources(args.Channel);

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

		if (m_Cooldowns.TryGetValue((args.Author.Id, args.Channel.Id), out DateTime lastSend) && DateTime.UtcNow - lastSend <= await channelConfigService.GetCooldownAsync(args.Channel)) {
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
					m_Logger.LogInformation("No matches found for {Ids}", string.Join(", ", ids));

					if (string.IsNullOrWhiteSpace(m_CoreConfig.CurrentValue.NoResultsEmote)) {
						return;
					}

					try {
						await args.Message.CreateReactionAsync(DiscordEmoji.FromName(m_Discord, m_CoreConfig.CurrentValue.NoResultsEmote, true));
					} catch (NotFoundException) { } catch (UnauthorizedException) { }

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
					PhotoType.General    => "Foto",
					PhotoType.Interior   => "Interieurfoto",
					PhotoType.Detail     => "Detailfoto",
					PhotoType.Cabin      => "Cabinefoto",
					PhotoType.EngineRoom => "Motorruimtefoto"
				};

				m_Cooldowns[(args.Message.Author.Id, args.Channel.Id)] = DateTime.UtcNow;
				try {
					await args.Message.RespondAsync(dmb => dmb.WithEmbed(new DiscordEmbedBuilder().WithAuthor(photobox.Photographer, photobox.PhotographerUrl)
						.WithTitle($"{typeName} van {photobox.Identity}")
						.WithUrl(photobox.PageUrl)
						.WithImageUrl(photobox.ImageUrl)
						.WithFooter($"© {photobox.Photographer}, {photobox.Taken} | Geen reacties meer? Blokkeer mij")));
				} catch (NotFoundException) {
					// Message was deleted
				}
			} catch (Exception ex) {
				FormattableString report = $"Error responding to message {args.Message.Id} ({args.Message.JumpLink}), numbers: {string.Join(", ", ids)}; photo url: {photobox?.PageUrl ?? "null"}";

				m_Logger.LogCritical(ex, report);

				if (m_Notifications != null) {
					await m_Notifications.SendNotificationAsync(report.ToString(), ex.Demystify());
				}
			}
		});
	}
}
