namespace TreinpositiesBot;

public abstract class PhotoSourceProvider {
	public abstract Task<List<string>?> GetSourceNamesForChannelAsync(ulong guildId, ulong channelId);
}
