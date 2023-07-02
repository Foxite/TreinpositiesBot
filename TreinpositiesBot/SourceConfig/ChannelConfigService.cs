namespace TreinpositiesBot.SourceConfig; 

public abstract class ChannelConfigService {
	public abstract Task<TimeSpan> GetCooldownAsync(ulong guildId, ulong channelId);
	public abstract Task<TimeSpan> GetSourcesAsync(ulong guildId, ulong channelId);
}
