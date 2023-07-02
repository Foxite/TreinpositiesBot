using Microsoft.EntityFrameworkCore;

namespace TreinpositiesBot.SourceConfig.Database;

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
