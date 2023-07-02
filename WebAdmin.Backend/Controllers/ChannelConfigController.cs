using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAdmin.Backend.Entities;

namespace WebAdmin.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class ChannelConfigController : ControllerBase {
	private readonly ILogger<ChannelConfigController> m_Logger;
	private readonly AppDbContext m_DbContext;

	public ChannelConfigController(ILogger<ChannelConfigController> logger, AppDbContext dbContext) {
		m_Logger = logger;
		m_DbContext = dbContext;
	}

	[HttpGet]
	[Route("{guildId:long}")]
	public async Task<IActionResult> GetGuild([FromRoute] ulong guildId) {
		Guild? guild = await m_DbContext.Guilds.FirstOrDefaultAsync(guild => guild.Id == guildId);
		List<Channel> channels = await m_DbContext.Channels.Where(channel => channel.GuildId == guildId).ToListAsync();

		return Ok(new {
			Id = guildId,
			CooldownSeconds = guild?.CooldownSeconds,
			SourceNames = guild?.SourceNames,
			Channels = channels,
		});
	}

	[HttpGet]
	[Route("{guildId:long}/{channelId:long}/Cooldown")]
	public async Task<IActionResult> GetCooldown([FromRoute] ulong guildId, [FromRoute] ulong channelId) {
		Guild? guild = await m_DbContext.Guilds.FindAsync(guildId);
		Channel? channel = await m_DbContext.Channels.FindAsync(guildId, channelId);
		
		//return Ok(channel?.CooldownSeconds.HasValue == true ? channel.CooldownSeconds : guild?.CooldownSeconds);
		return Ok(channel?.CooldownSeconds ?? guild?.CooldownSeconds);
	}

	[HttpGet]
	[Route("{guildId:long}/{channelId:long}/Sources")]
	public async Task<IActionResult> GetSources([FromRoute] ulong guildId, [FromRoute] ulong channelId) {
		Guild? guild = await m_DbContext.Guilds.FindAsync(guildId);
		Channel? channel = await m_DbContext.Channels.FindAsync(guildId, channelId);
		
		return Ok(channel?.SourceNames ?? guild?.SourceNames);
	}

	[HttpPut]
	[Route("{guildId:long}/{channelId:long}/Cooldown")]
	public async Task<IActionResult> SetChannelCooldown([FromRoute] ulong guildId, [FromRoute] ulong channelId, [FromBody] float cooldownSeconds) {
		Channel? channel = m_DbContext.Channels.FirstOrDefault(channel => channel.GuildId == guildId && channel.Id == channelId);
		if (channel == null) {
			channel = new Channel() {
				GuildId = guildId,
				Id = channelId,
				CooldownSeconds = cooldownSeconds,
			};

			m_DbContext.Channels.Add(channel);
		} else {
			channel.CooldownSeconds = cooldownSeconds;
		}
		
		await m_DbContext.SaveChangesAsync();

		return NoContent();
	}

	[HttpPut]
	[Route("{guildId:long}/{channelId:long}/Sources")]
	public async Task<IActionResult> SetChannelSources([FromRoute] ulong guildId, [FromRoute] ulong channelId, [FromBody] string[] sources) {
		Channel? channel = m_DbContext.Channels.FirstOrDefault(channel => channel.GuildId == guildId && channel.Id == channelId);
		if (channel == null) {
			channel = new Channel() {
				GuildId = guildId,
				Id = channelId,
				SourceNames = sources,
			};

			m_DbContext.Channels.Add(channel);
		} else {
			channel.SourceNames = sources;
		}
		
		await m_DbContext.SaveChangesAsync();

		return NoContent();
	}

	[HttpPut]
	[Route("{guildId:long}/Cooldown")]
	public async Task<IActionResult> SetGuildCooldown([FromRoute] ulong guildId, [FromBody] float cooldownSeconds) {
		Guild? guild = await m_DbContext.Guilds.FindAsync(guildId);
		if (guild == null) {
			guild = new Guild() {
				Id = guildId,
				CooldownSeconds = cooldownSeconds,
			};

			m_DbContext.Guilds.Add(guild);
		} else {
			guild.CooldownSeconds = cooldownSeconds;
		}
		
		await m_DbContext.SaveChangesAsync();

		return NoContent();
	}

	[HttpPut]
	[Route("{guildId:long}/Sources")]
	public async Task<IActionResult> SetGuildCooldown([FromRoute] ulong guildId, [FromBody] string[] sources) {
		Guild? guild = await m_DbContext.Guilds.FindAsync(guildId);
		if (guild == null) {
			guild = new Guild() {
				Id = guildId,
				SourceNames = sources,
			};

			m_DbContext.Guilds.Add(guild);
		} else {
			guild.SourceNames = sources;
		}
		
		await m_DbContext.SaveChangesAsync();

		return NoContent();
	}
}
