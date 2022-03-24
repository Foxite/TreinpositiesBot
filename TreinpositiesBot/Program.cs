using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using DSharpPlus;

var discord = new DiscordClient(new DiscordConfiguration() {
	Token = Environment.GetEnvironmentVariable("BOT_TOKEN"),
	Intents = DiscordIntents.GuildMessages
});

var regex = new Regex(@"\d{3,6}");

var http = new HttpClient() {
	DefaultRequestHeaders = {
		UserAgent = { new ProductInfoHeaderValue("TreinpositiesBot", "0.1") }
	}
};

async Task LookupTrainPicsAndSend(string[] numbers) {
	
}

discord.MessageCreated += (unused, args) => {
	MatchCollection matches = regex.Matches(args.Message.Content);
	if (matches.Count > 0) {
		_ = LookupTrainPicsAndSend(matches.Cast<Match>().Select(match => match.Value).ToArray());
	}
	return Task.CompletedTask;
};

await discord.ConnectAsync();
