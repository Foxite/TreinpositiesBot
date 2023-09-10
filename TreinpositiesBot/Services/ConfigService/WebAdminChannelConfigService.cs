using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using DSharpPlus.Entities;
using Microsoft.Extensions.Options;

namespace TreinpositiesBot; 

public class WebAdminChannelConfigService : ChannelConfigService {
	private readonly WebAdminService m_WebAdminService;
	
	public WebAdminChannelConfigService(IOptionsMonitor<CoreConfig> options, WebAdminService webAdminService) : base(options) {
		m_WebAdminService = webAdminService;
	}
	
	protected async override Task<TimeSpan?> InternalGetCooldownAsync(DiscordChannel channel) {
		int? cooldownSeconds = await m_WebAdminService.GetConfigValue<int?>("Cooldown", channel);
		if (cooldownSeconds.HasValue) {
			return TimeSpan.FromSeconds(cooldownSeconds.Value);
		} else {
			return default;
		}
	}
	
	protected override Task<ICollection<string>?> InternalGetSourceNamesAsync(DiscordChannel channel) => m_WebAdminService.GetConfigValue<ICollection<string>>("SourceNames", channel);
}

public class WebAdminService {
	private readonly IOptions<CoreConfig> m_Options;
	private readonly HttpClient m_HttpClient;
	
	public WebAdminService(HttpClient httpClient, IOptions<CoreConfig> options1) {
		m_HttpClient = httpClient;
		m_Options = options1;
	}

	public Task<T?> GetConfigValue<T>(string key, DiscordChannel channel) {
		return GetConfigValue<T>(key, GetLevels(channel));
	}

	public async Task<T?> GetConfigValue<T>(string key, IEnumerable<string> levels) {
		ConfigItem<T>? ret = await GetConfigItem<T>(key, levels);

		if (ret == null) {
			return default;
		} else {
			return ret.Value;
		}
	}

	public Task<ConfigItem<T>?> GetConfigItem<T>(string key, DiscordChannel channel) {
		return GetConfigItem<T>(key, GetLevels(channel));
	}

	public async Task<ConfigItem<T>?> GetConfigItem<T>(string key, IEnumerable<string> levels) {
		string url = $"{m_Options.Value.AdminBackendUrl}/ConfigKey/{string.Join(":", levels)}/{key}";
		using HttpResponseMessage response = await m_HttpClient.GetAsync(url);
		if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.NoContent) {
			return null;
		} else {
			return await response.Content.ReadFromJsonAsync<ConfigItem<T>>();
		}
	}

	private IEnumerable<string> GetLevels(DiscordChannel channel) {
		var levels = new Stack<string>();
		levels.Push(channel.Id.ToString());
		
		Debug.Assert((channel.Parent != null) == channel.ParentId.HasValue);
		
		while (channel.Parent != null) {
			channel = channel.Parent;
			levels.Push(channel!.Id.ToString());
			
			Debug.Assert((channel.Parent != null) == channel.ParentId.HasValue);
		}

		if (channel.GuildId.HasValue) {
			levels.Push(channel.GuildId.Value.ToString());
		}

		return levels;
	}
}

public record ConfigItem<T>(string OverrideLevel, T Value);
