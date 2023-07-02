using DSharpPlus.Entities;
using Microsoft.Extensions.Options;

namespace TreinpositiesBot;

public class ConfigChannelConfigService : ChannelConfigService {
	private readonly IOptionsMonitor<CoreConfig> m_Options;
	
	public ConfigChannelConfigService(IOptionsMonitor<CoreConfig> options) : base(options) {
		m_Options = options;
	}

	protected override Task<TimeSpan?> InternalGetCooldownAsync(DiscordChannel channel) {
		if (m_Options.CurrentValue.Guilds == null) {
			return Task.FromResult<TimeSpan?>(null);
		}

		if (m_Options.CurrentValue.Guilds.TryGetValue(channel.Guild.Id, out GuildConfig? guildConfig)) {
			if (guildConfig.Channels == null) {
				return Task.FromResult(guildConfig.Cooldown);
			}

			if (guildConfig.Channels.TryGetValue(channel.Id, out ChannelConfig? channelConfig)) {
				return Task.FromResult(channelConfig.Cooldown ?? guildConfig.Cooldown);
			}

			return Task.FromResult(guildConfig.Cooldown);
		} else {
			return Task.FromResult<TimeSpan?>(null);
		}
	}

	protected override Task<ICollection<string>?> InternalGetSourceNamesAsync(DiscordChannel channel) {
		if (m_Options.CurrentValue.Guilds == null) {
			return Task.FromResult<ICollection<string>?>(null);
		}

		if (m_Options.CurrentValue.Guilds.TryGetValue(channel.Guild.Id, out GuildConfig? guildConfig)) {
			if (guildConfig.Channels == null) {
				return Task.FromResult<ICollection<string>?>(guildConfig.Sources);
			}

			if (guildConfig.Channels.TryGetValue(channel.Id, out ChannelConfig? channelConfig)) {
				return Task.FromResult<ICollection<string>?>(channelConfig.Sources ?? guildConfig.Sources);
			}
		}
		
		return Task.FromResult<ICollection<string>?>(null);
	}
}
