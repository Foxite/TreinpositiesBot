namespace WebAdmin.Backend.Config; 

public class RedisConfig {
	public IList<string> Endpoints { get; set; } = new List<string>();
	public int Database { get; set; }
}
