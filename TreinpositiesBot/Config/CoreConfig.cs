namespace TreinpositiesBot; 

public class CoreConfig {
	public string DiscordToken { get; set; }
	public string? ErrorWebhookUrl { get; set; }
	public string? NoResultsEmote { get; set; }
	public int CooldownSeconds { get; set; }
	public Dictionary<ulong, GuildConfig>? Guilds { get; set; }

	public TimeSpan Cooldown => TimeSpan.FromSeconds(CooldownSeconds);

	public TimeSpan GetCooldown(ulong? guildId, ulong channelId) {
		if (!guildId.HasValue || Guilds == null) {
			return Cooldown;
		}

		if (Guilds.TryGetValue(guildId.Value, out GuildConfig? guildConfig)) {
			return guildConfig.GetCooldown(channelId) ?? Cooldown;
		} else {
			return Cooldown;
		}
	}

	public List<string>? GetSources(ulong? guildId, ulong channelId) {
		if (!guildId.HasValue || Guilds == null) {
			return null;
		}

		if (Guilds.TryGetValue(guildId.Value, out GuildConfig? guildConfig)) {
			return guildConfig.GetSources(channelId) ?? null;
		} else {
			return null;
		}
	}
}

public class GuildConfig {
	public int? CooldownSeconds { get; set; }
	public List<string>? Sources { get; set; }
	
	public Dictionary<ulong, ChannelConfig>? Channels { get; set; }

	public TimeSpan? Cooldown => CooldownSeconds.HasValue ? TimeSpan.FromSeconds(CooldownSeconds.Value) : null;

	public TimeSpan? GetCooldown(ulong channelId) {
		if (Channels == null) {
			return Cooldown;
		}

		if (Channels.TryGetValue(channelId, out ChannelConfig? channelConfig)) {
			return channelConfig.Cooldown ?? Cooldown;
		}

		return null;
	}

	public List<string>? GetSources(ulong channelId) {
		if (Channels == null) {
			return Sources;
		}

		if (Channels.TryGetValue(channelId, out ChannelConfig? channelConfig)) {
			return channelConfig.Sources ?? Sources;
		}

		return null;
	}
}

public class ChannelConfig {
	public int? CooldownSeconds { get; set; }
	public List<string>? Sources { get; set; }

	public TimeSpan? Cooldown => CooldownSeconds.HasValue ? TimeSpan.FromSeconds(CooldownSeconds.Value) : null;
}
