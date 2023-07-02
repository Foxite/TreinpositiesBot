namespace TreinpositiesBot.SourceConfig.Database;

public class ServerConfig {
	public ulong Id { get; set; }
	public ICollection<ChannelConfig> Channels { get; set; }
	public List<string> DefaultSources { get; set; }
}
