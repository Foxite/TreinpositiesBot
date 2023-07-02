using Microsoft.EntityFrameworkCore;

namespace WebAdmin.Backend.Entities; 

public class AppDbContext : DbContext {
	public DbSet<Guild> Guilds { get; set; }
	public DbSet<Channel> Channels { get; set; }
	
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {} 
	
	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		modelBuilder
			.Entity<Channel>()
			.HasKey(channel => new { channel.GuildId, channel.Id });
	}
}
