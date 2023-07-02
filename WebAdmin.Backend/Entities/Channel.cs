namespace WebAdmin.Backend.Entities;

public class Channel {
	public ulong GuildId { get; set; }
	public ulong Id { get; set; }

	public float? CooldownSeconds { get; set; }
	public string[]? SourceNames { get; set; }
}
