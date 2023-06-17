using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace TreinpositiesBot; 

public class PhotoSourceService {
	private readonly PhotoSourceProvider m_Provider;
	private readonly IEnumerable<PhotoSource> m_Sources;

	public PhotoSourceService(PhotoSourceProvider provider, IEnumerable<PhotoSource> sources) {
		m_Provider = provider;
		m_Sources = sources;
	}

	public async Task<List<PhotoSource>> GetPhotoSourcesAsync(DiscordChannel channel) {
		List<PhotoSource>? ret = null;
		if (channel.GuildId.HasValue) {
			List<string>? sourceNames = await m_Provider.GetSourceNamesForChannelAsync(channel.GuildId.Value, channel.Id);
			if (sourceNames != null) {
				ret = m_Sources.Where(source => sourceNames.Contains(source.Name)).ToList();
			}
		}
		
		if (ret == null) {
			ret = m_Sources.ToList();
			ret.Shuffle();
		}

		return ret;
	}
}

public abstract class PhotoSourceProvider {
	public abstract Task<List<string>?> GetSourceNamesForChannelAsync(ulong guildId, ulong channelId);
}

public class ConfigPhotoSourceProvider : PhotoSourceProvider {
	private readonly IOptionsMonitor<SourcesConfig> m_Sources;

	public ConfigPhotoSourceProvider(IOptionsMonitor<SourcesConfig> sources) {
		m_Sources = sources;
	}

	public override Task<List<string>?> GetSourceNamesForChannelAsync(ulong guildId, ulong channelId) {
		if (m_Sources.CurrentValue.SourcesByGuild == null) {
			var ret = Task.FromResult<List<string>?>(null);
			return ret;
		} else {
			m_Sources.CurrentValue.SourcesByGuild.TryGetValue(guildId, out List<string>? sourceNames);
			return Task.FromResult(sourceNames);
		}
	}
}

public class PostgresPhotoSourceProvider : PhotoSourceProvider {
	private readonly PhotoSourceDbContext m_DbContext;

	public PostgresPhotoSourceProvider(PhotoSourceDbContext dbContext) {
		m_DbContext = dbContext;
	}

	public async override Task<List<string>?> GetSourceNamesForChannelAsync(ulong guildId, ulong channelId) {
		ChannelConfig? channelConfig = await m_DbContext.ChannelConfigs.FindAsync(guildId, channelId);
		if (channelConfig != null) {
			return channelConfig.Sources;
		} else {
			ServerConfig? serverConfig = await m_DbContext.ServerConfigs.FindAsync(guildId);
			return serverConfig?.DefaultSources;
		}
	}
}

public class PhotoSourceDbContext : DbContext {
	public DbSet<ServerConfig> ServerConfigs { get; set; }
	public DbSet<ChannelConfig> ChannelConfigs { get; set; }

	public PhotoSourceDbContext() : base() { }
	public PhotoSourceDbContext(DbContextOptions<PhotoSourceDbContext> dbco) : base(dbco) { }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
		if (!optionsBuilder.IsConfigured) {
			// For `dotnet ef`
			optionsBuilder.UseNpgsql("Host=database; Port=5432; Username=tpbot; Password=tpbot");
		}
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		base.OnModelCreating(modelBuilder);
		modelBuilder.Entity<ChannelConfig>().HasKey(nameof(ChannelConfig.Server), nameof(ChannelConfig.Id));
	}
}

public class ServerConfig {
	public ulong Id { get; set; }
	public ICollection<ChannelConfig> Channels { get; set; }
	public List<string> DefaultSources { get; set; }
}

public class ChannelConfig {
	public ulong Id { get; set; }
	public ServerConfig Server { get; set; }
	public List<string> Sources { get; set; }
}
