namespace TreinpositiesBot.SourceConfig.Database;

public class ChannelConfig {
	public ulong Id { get; set; }
	public ServerConfig Server { get; set; }
	public List<string> Sources { get; set; }
}
