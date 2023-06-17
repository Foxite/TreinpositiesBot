using Microsoft.Extensions.Options;

namespace TreinpositiesBot;

public class ConfigPhotoSourceProvider : PhotoSourceProvider {
	private readonly IOptionsMonitor<SourcesConfig> m_Sources;

	public ConfigPhotoSourceProvider(IOptionsMonitor<SourcesConfig> sources) {
		m_Sources = sources;
	}

	public override Task<List<string>?> GetSourceNamesForChannelAsync(ulong guildId, ulong channelId) {
		if (m_Sources.CurrentValue.SourcesByGuild == null) {
			var ret = Task.FromResult<List<string>?>(null);
			return ret;
		} else {
			m_Sources.CurrentValue.SourcesByGuild.TryGetValue(guildId, out List<string>? sourceNames);
			return Task.FromResult(sourceNames);
		}
	}
}
