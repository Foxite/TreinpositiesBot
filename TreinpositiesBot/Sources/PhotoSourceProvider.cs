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
		if (channel.GuildId.HasValue) {
			List<string>? guildSources = m_Config.CurrentValue.GetSources(channel.GuildId, channel.Id);
			if (guildSources != null) {
				ret = m_Sources.Where(source => guildSources.Contains(source.Name)).ToList();
			} else {
				ret = m_Sources.ToList();
			}
		} else {
			ret = m_Sources.ToList();
		}

		ret.Shuffle();
		return ret;
	}
}
