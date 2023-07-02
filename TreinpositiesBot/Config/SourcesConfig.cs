namespace TreinpositiesBot.Config; 

public class SourcesConfig {
	public SourcesConfigSource Source { get; set; }
	public Dictionary<ulong, GuildConfig>? Guilds { get; set; }
	public string? ConnectionString { get; set; }
}

public enum SourcesConfigSource {
	Config,
	Postgres
}
