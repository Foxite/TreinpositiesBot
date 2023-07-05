using Discord;
using Discord.Rest;
using Microsoft.EntityFrameworkCore;
using WebAdmin.Backend.Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(dbcob => dbcob.UseNpgsql(builder.Configuration.GetValue<string>("Database")));

builder.Services.AddSingleton<DiscordRestClient>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope()) {
	await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	await dbContext.Database.MigrateAsync();
}

var discord = app.Services.GetRequiredService<DiscordRestClient>();
await discord.LoginAsync(TokenType.Bot, app.Services.GetRequiredService<IConfiguration>().GetValue<string>("DiscordToken"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI();
}

string[]? allowedCorsOrigins = app.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>();
if (allowedCorsOrigins != null) {
	app.UseCors(cpb => cpb.WithOrigins(allowedCorsOrigins));
}

app.UseAuthorization();

app.MapControllers();

app.Run();
