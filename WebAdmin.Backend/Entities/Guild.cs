namespace WebAdmin.Backend.Entities;

public class Guild {
	public ulong Id { get; set; }

	public float? CooldownSeconds { get; set; }
	public string[]? SourceNames { get; set; }
}
