using DSharpPlus.Entities;
using Microsoft.Extensions.Options;

namespace TreinpositiesBot; 

public abstract class ChannelConfigService {
	private readonly IOptionsMonitor<CoreConfig> m_Options;

	protected ChannelConfigService(IOptionsMonitor<CoreConfig> options) {
		m_Options = options;
	}

	public async Task<TimeSpan> GetCooldownAsync(DiscordChannel channel) {
		if (channel.Guild == null) {
			return m_Options.CurrentValue.Cooldown;
		}
		
		return await InternalGetCooldownAsync(channel) ?? m_Options.CurrentValue.Cooldown;
	}

	public async Task<ICollection<string>?> GetSourceNamesAsync(DiscordChannel channel) {
		if (channel.Guild == null) {
			return null;
		}
		
		return await InternalGetSourceNamesAsync(channel);
	}
	
	protected abstract Task<TimeSpan?> InternalGetCooldownAsync(DiscordChannel channel);
	protected abstract Task<ICollection<string>?> InternalGetSourceNamesAsync(DiscordChannel channel);
}
