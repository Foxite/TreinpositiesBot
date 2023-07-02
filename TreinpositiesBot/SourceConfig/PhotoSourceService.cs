using DSharpPlus.Entities;

namespace TreinpositiesBot.SourceConfig;

public class PhotoSourceService {
	private readonly PhotoSourceProvider m_Provider;
	private readonly IEnumerable<PhotoSource> m_Sources;

	public PhotoSourceService(PhotoSourceProvider provider, IEnumerable<PhotoSource> sources) {
		m_Provider = provider;
		m_Sources = sources;
	}

	public async Task<List<PhotoSource>> GetPhotoSourcesAsync(DiscordChannel channel) {
		List<PhotoSource>? ret = null;
		if (channel.GuildId.HasValue) {
			List<string>? sourceNames = await m_Provider.GetSourceNamesForChannelAsync(channel.GuildId.Value, channel.Id);
			if (sourceNames != null) {
				ret = m_Sources.Where(source => sourceNames.Contains(source.Name)).ToList();
			}
		}

		return ret ?? m_Sources.ToList();
	}
}
