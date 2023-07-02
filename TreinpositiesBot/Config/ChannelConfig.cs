namespace TreinpositiesBot.Config;

public class ChannelConfig {
	public int? CooldownSeconds { get; set; }
	public List<string>? Sources { get; set; }

	public TimeSpan? Cooldown => CooldownSeconds.HasValue ? TimeSpan.FromSeconds(CooldownSeconds.Value) : null;
}
