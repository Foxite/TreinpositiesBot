namespace TreinpositiesBot;

public class GuildConfig {
	public int? CooldownSeconds { get; set; }
	public List<string>? Sources { get; set; }
	
	public Dictionary<ulong, ChannelConfig>? Channels { get; set; }

	public TimeSpan? Cooldown => CooldownSeconds.HasValue ? TimeSpan.FromSeconds(CooldownSeconds.Value) : null;
}
