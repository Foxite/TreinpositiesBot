using Discord;
using Discord.Rest;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WebAdmin.Backend.Config;
 
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RedisConfig>(builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton(isp => {
	IList<string> endpoints = isp.GetRequiredService<IOptions<RedisConfig>>().Value.Endpoints;

	var connectionOptions = new ConfigurationOptions();
	foreach (string endpoint in endpoints) {
		connectionOptions.EndPoints.Add(endpoint);
	}
	
	return ConnectionMultiplexer.Connect(connectionOptions);
});

builder.Services.AddSingleton<DiscordRestClient>();

var app = builder.Build();

var connectionMultiplexer = app.Services.GetRequiredService<ConnectionMultiplexer>();
Console.WriteLine(await connectionMultiplexer.GetDatabase().PingAsync());

var discord = app.Services.GetRequiredService<DiscordRestClient>();
await discord.LoginAsync(TokenType.Bot, app.Services.GetRequiredService<IConfiguration>().GetValue<string>("DiscordToken"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI();
}

string[]? allowedCorsOrigins = app.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>();
if (allowedCorsOrigins != null) {
	app.UseCors(cpb => cpb
		.WithOrigins(allowedCorsOrigins)
		.AllowAnyMethod()
		.AllowAnyHeader()
	);
}

app.UseAuthorization();

app.MapControllers();

app.Run();
