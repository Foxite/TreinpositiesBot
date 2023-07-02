using Microsoft.Extensions.Options;
using TreinpositiesBot.Config;

namespace TreinpositiesBot.SourceConfig;

public class ConfigPhotoSourceProvider : PhotoSourceProvider {
	private readonly IOptionsMonitor<SourcesConfig> m_Sources;

	public ConfigPhotoSourceProvider(IOptionsMonitor<SourcesConfig> sources) {
		m_Sources = sources;
	}

	public override Task<List<string>?> GetSourceNamesForChannelAsync(ulong guildId, ulong channelId) {
		if (m_Sources.CurrentValue.Guilds != null && m_Sources.CurrentValue.Guilds.TryGetValue(guildId, out GuildConfig? guildConfig)) {
			if (guildConfig.Channels != null && guildConfig.Channels.TryGetValue(channelId, out ChannelConfig? channelConfig)) {
				return Task.FromResult(channelConfig.Sources ?? guildConfig.Sources);
			} else {
				return Task.FromResult(guildConfig.Sources);
			}
		}
		return Task.FromResult<List<string>?>(null);
	}
}
