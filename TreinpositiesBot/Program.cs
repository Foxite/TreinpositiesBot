using System.Diagnostics;
using System.Net.Http.Headers;
using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TreinpositiesBot;

var host = Host.CreateDefaultBuilder()
	.ConfigureAppConfiguration((hbc, icb) => {
		icb.AddJsonFile("appsettings.json");
		icb.AddJsonFile($"appsettings.{hbc.HostingEnvironment.EnvironmentName}.json", true);
		icb.AddEnvironmentVariables();
		icb.AddCommandLine(args);
	})
	.ConfigureLogging((hbc, ilb) => {
		//ilb.AddSimpleConsole();
	})
	.ConfigureServices((hbc, isc) => {
		isc.Configure<CoreConfig>(hbc.Configuration.GetSection("Core"));
		isc.Configure<TreinpositiesPhotoSource.Options>(hbc.Configuration.GetSection("Treinposities"));
		isc.Configure<JetphotosPhotoSource.Options>(hbc.Configuration.GetSection("Jetphotos"));

		isc.AddSingleton<Random>();	
		
		isc.AddSingleton(isp => new DiscordClient(new DiscordConfiguration() {
			Token = isp.GetRequiredService<IOptions<CoreConfig>>().Value.DiscordToken,
			Intents = DiscordIntents.GuildMessages | DiscordIntents.Guilds,
			LoggerFactory = isp.GetRequiredService<ILoggerFactory>(),
		}));

		isc.AddSingleton(_ => new HttpClientHandler() {
			AllowAutoRedirect = false,
		});

		isc.AddSingleton(isp => {
			var ret = new HttpClient(isp.GetRequiredService<HttpClientHandler>());

			ret.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TreinpositiesBot", "0.5"));
			ret.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Foxite/TreinpositiesBot)"));

			return ret;
		});
		
		isc.TryAddEnumerable(ServiceDescriptor.Singleton<PhotoSource, TreinBusPositiesPhotoSource>());
		isc.TryAddEnumerable(ServiceDescriptor.Singleton<PhotoSource, BuspositiesPhotoSource>());
		isc.TryAddEnumerable(ServiceDescriptor.Singleton<PhotoSource, TreinpositiesPhotoSource>());
		isc.TryAddEnumerable(ServiceDescriptor.Singleton<PhotoSource, PlanespottersScrapingPhotoSource>());
		isc.TryAddEnumerable(ServiceDescriptor.Singleton<PhotoSource, JetphotosPhotoSource>());

		isc.AddSingleton<PhotoSourceProvider>();

		isc.AddSingleton<ChannelConfigService, ConfigChannelConfigService>();

		isc.AddHostedService<AppService>();
	})
	.Build();


var discord = host.Services.GetRequiredService<DiscordClient>();
var logger = host.Services.GetRequiredService<ILogger<AppService>>();
discord.ClientErrored += (_, args) => {
	logger.LogError(args.Exception.Demystify(), "Discord client error");
	return Task.CompletedTask;
};

await discord.ConnectAsync();

await host.RunAsync();
