namespace TreinpositiesBot.SourceConfig.Database;

public class DatabasePhotoSourceProvider : PhotoSourceProvider {
	private readonly PhotoSourceDbContext m_DbContext;

	public DatabasePhotoSourceProvider(PhotoSourceDbContext dbContext) {
		m_DbContext = dbContext;
	}

	public async override Task<List<string>?> GetSourceNamesForChannelAsync(ulong guildId, ulong channelId) {
		ChannelConfig? channelConfig = await m_DbContext.ChannelConfigs.FindAsync(guildId, channelId);
		if (channelConfig != null) {
			return channelConfig.Sources;
		} else {
			ServerConfig? serverConfig = await m_DbContext.ServerConfigs.FindAsync(guildId);
			return serverConfig?.DefaultSources;
		}
	}
}
