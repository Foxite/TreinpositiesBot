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
