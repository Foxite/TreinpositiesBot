namespace TreinpositiesBot.Config; 

public class CoreConfig {
	public string DiscordToken { get; set; }
	public string? ErrorWebhookUrl { get; set; }
	public string? NoResultsEmote { get; set; }
	public int CooldownSeconds { get; set; }

	public TimeSpan Cooldown => TimeSpan.FromSeconds(CooldownSeconds);
}
