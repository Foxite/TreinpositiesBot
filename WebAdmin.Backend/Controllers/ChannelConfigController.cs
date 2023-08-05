using System.Collections;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAdmin.Backend.Entities;

namespace WebAdmin.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class ChannelConfigController : ControllerBase {
	private readonly ILogger<ChannelConfigController> m_Logger;
	private readonly AppDbContext m_DbContext;
	private readonly DiscordRestClient m_Discord;

	public ChannelConfigController(ILogger<ChannelConfigController> logger, AppDbContext dbContext, DiscordRestClient discord) {
		m_Logger = logger;
		m_DbContext = dbContext;
		m_Discord = discord;
	}

	[HttpGet]
	[Route("{guildId:long}")]
	public async Task<IActionResult> GetGuild([FromRoute] ulong guildId) {
		RestGuild? discordGuild = await m_Discord.GetGuildAsync(guildId);
		
		if (discordGuild == null) {
			return NotFound();
		}

		var entityChannels = m_DbContext.Channels.Where(entityChannel => entityChannel.GuildId == guildId).ToList();
		
		IEnumerable<GuildChannelConfig> GetGuildChannelConfigs(IEnumerable<IGuildChannel> discordChannels) =>
			from discordChannel in discordChannels
			orderby discordChannel.GetChannelType() is ChannelType.Stage or ChannelType.Voice, discordChannel.Position
			where discordChannel is not RestCategoryChannel
			let entityChannel = entityChannels.FirstOrDefault(entityChannel => entityChannel.Id == discordChannel.Id) ?? new Channel() {
				Id = discordChannel.Id,
			}
			select new GuildChannelConfig(
				discordChannel.Id.ToString(),
				discordChannel.Name,
				discordChannel.GetChannelType().Value,
				entityChannel.CooldownSeconds,
				entityChannel.SourceNames
			);

		IReadOnlyCollection<RestGuildChannel>? allChannels = await discordGuild.GetChannelsAsync();
		IReadOnlyCollection<INestedChannel> discordChannels = (allChannels).OfType<INestedChannel>().ToList();
		Guild guild = (await m_DbContext.Guilds.FirstOrDefaultAsync(guild => guild.Id == guildId)) ?? new Guild() {
			Id = guildId,
		};

		List<GuildChannelCategory> channelCategories = discordChannels
			.GroupBy(channel => channel.CategoryId)
			.Select(grouping => new {
				Position = grouping.Key.HasValue ? allChannels.First(rgc => rgc.Id == grouping.Key.Value).Position : -1,
				Gcc = new GuildChannelCategory(
					grouping.Key?.ToString(),
					grouping.Key.HasValue ? allChannels.First(rgc => rgc.Id == grouping.Key.Value).Name : null,
					GetGuildChannelConfigs(grouping).ToList()
				),
			})
			.OrderBy(item => item.Position)
			.Select(item => item.Gcc)
			.ToList();
		
		return Ok(new GuildConfig(
			guildId.ToString(),
			guild.CooldownSeconds,
			guild.SourceNames,
			channelCategories
		));
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
	public async Task<IActionResult> SetChannelCooldown([FromRoute] ulong guildId, [FromRoute] ulong channelId, [FromBody] float? cooldownSeconds) {
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
	public async Task<IActionResult> SetChannelSources([FromRoute] ulong guildId, [FromRoute] ulong channelId, [FromBody] string[]? sources) {
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
	public async Task<IActionResult> SetGuildCooldown([FromRoute] ulong guildId, [FromBody] float? cooldownSeconds) {
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
	public async Task<IActionResult> SetGuildCooldown([FromRoute] ulong guildId, [FromBody] string[]? sources) {
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

public record GuildConfig(
	string Id,
	float? CooldownSeconds,
	string[]? SourceNames,
	IList<GuildChannelCategory> Categories
);

public record GuildChannelCategory(
	string? Id,
	string? Name,
	IList<GuildChannelConfig> Channels
);

public record GuildChannelConfig(
	string Id,
	string Name,
	ChannelType Type,
	float? CooldownSeconds,
	string[]? SourceNames
);
