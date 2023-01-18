namespace TreinpositiesBot; 

public class SourcesConfig {
	public SourcesConfigSource Source { get; set; }
	public Dictionary<ulong, List<string>>? SourcesByGuild { get; set; }
	public string? ConnectionString { get; set; }
}

public enum SourcesConfigSource {
	Config,
	Postgres
}
