namespace TreinpositiesBot; 

public class CoreConfig {
	public string DiscordToken { get; set; }
	public string? ErrorWebhookUrl { get; set; }
	public string? NoResultsEmote { get; set; }
	public int CooldownSeconds { get; set; }
	public Dictionary<ulong, GuildConfig>? Guilds { get; set; }

	public TimeSpan Cooldown => TimeSpan.FromSeconds(CooldownSeconds);
}
