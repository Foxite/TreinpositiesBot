using DSharpPlus.Entities;
using Microsoft.Extensions.Options;

namespace TreinpositiesBot; 

public class PhotoSourceProvider {
	private readonly IOptionsMonitor<CoreConfig> m_Config;
	private readonly IEnumerable<PhotoSource> m_Sources;

	public PhotoSourceProvider(IOptionsMonitor<CoreConfig> config, IEnumerable<PhotoSource> sources) {
		m_Config = config;
		m_Sources = sources;
	}

	public List<PhotoSource> GetPhotoSources(DiscordChannel channel) {
		List<PhotoSource> ret;
		if (channel.GuildId.HasValue && m_Config.CurrentValue.SourcesByGuild != null && m_Config.CurrentValue.SourcesByGuild.TryGetValue(channel.GuildId.Value, out List<string>? sourceNames)) {
			ret = m_Sources.Where(source => sourceNames.Contains(source.Name)).ToList();
			return ret;
		} else {
			ret = m_Sources.ToList();
		}

		ret.Shuffle();
		return ret;
	}
}
