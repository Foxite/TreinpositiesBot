using DSharpPlus.Entities;
using Microsoft.Extensions.Options;

namespace TreinpositiesBot; 

public class PhotoSourceProvider {
	private readonly ChannelConfigService m_ChannelConfigService;
	private readonly IEnumerable<PhotoSource> m_Sources;

	public PhotoSourceProvider(ChannelConfigService channelConfigService, IEnumerable<PhotoSource> sources) {
		m_ChannelConfigService = channelConfigService;
		m_Sources = sources;
	}

	public async Task<List<PhotoSource>> GetPhotoSources(DiscordChannel channel) {
		List<PhotoSource> ret;
		if (channel.GuildId.HasValue) {
			ICollection<string>? sourceNames = await m_ChannelConfigService.GetSourceNamesAsync(channel);
			if (sourceNames != null) {
				ret = m_Sources.Where(source => sourceNames.Contains(source.Name)).ToList();
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
