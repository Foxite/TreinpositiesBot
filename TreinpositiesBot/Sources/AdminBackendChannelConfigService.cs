using System.Net;
using System.Net.Http.Json;
using DSharpPlus.Entities;
using Microsoft.Extensions.Options;

namespace TreinpositiesBot; 

public class AdminBackendChannelConfigService : ChannelConfigService {
	private readonly IOptions<CoreConfig> m_Options;
	private readonly HttpClient m_HttpClient;
	
	public AdminBackendChannelConfigService(IOptionsMonitor<CoreConfig> options, HttpClient httpClient, IOptions<CoreConfig> options1) : base(options) {
		m_HttpClient = httpClient;
		m_Options = options1;
	}

	protected async override Task<TimeSpan?> InternalGetCooldownAsync(DiscordChannel channel) {
		string url = $"{m_Options.Value.AdminBackendUrl}/ChannelConfig/{channel.Guild.Id}/{channel.Id}/Cooldown";
		using HttpResponseMessage response = await m_HttpClient.GetAsync(url);
		if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.NoContent) {
			return null;
		} else {
			response.EnsureSuccessStatusCode();
			return TimeSpan.FromSeconds(int.Parse(await response.Content.ReadAsStringAsync()));
		}
	}
	
	protected async override Task<ICollection<string>?> InternalGetSourceNamesAsync(DiscordChannel channel) {
		string url = $"{m_Options.Value.AdminBackendUrl}/ChannelConfig/{channel.Guild.Id}/{channel.Id}/Sources";
		using HttpResponseMessage response = await m_HttpClient.GetAsync(url);
		if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.NoContent) {
			return null;
		} else {
			return await response.Content.ReadFromJsonAsync<string[]>();
		}
	}
}
