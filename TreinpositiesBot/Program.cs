using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Foxite.Common.Notifications;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;

var discord = new DiscordClient(new DiscordConfiguration() {
	Token = Environment.GetEnvironmentVariable("BOT_TOKEN"),
	Intents = DiscordIntents.GuildMessages | DiscordIntents.Guilds
});

discord.ClientErrored += (_, args) => {
	Console.WriteLine(args.Exception.ToStringDemystified());
	return Task.CompletedTask;
};

var handler = new HttpClientHandler();
handler.AllowAutoRedirect = false;
var http = new HttpClient(handler) {
	DefaultRequestHeaders = {
		UserAgent = {
			new ProductInfoHeaderValue("TreinpositiesBot", "0.2"),
			new ProductInfoHeaderValue("(https://github.com/Foxite/TreinpositiesBot)")
		}
	},
	BaseAddress = new Uri("https://treinposities.nl/")
};

string[] blockedPhotographers = (Environment.GetEnvironmentVariable("BLOCKED_PHOTOGRAPHERS") ?? "").Split(";");

var random = new Random();

DiscordWebhookLibNotificationSender? notifications = null;
string? webhookUrl = Environment.GetEnvironmentVariable("WEBHOOK_URL");
if (webhookUrl != null) {
	notifications = new DiscordWebhookLibNotificationSender(new OptionsWrapper<DiscordWebhookLibNotificationSender.Config>(new DiscordWebhookLibNotificationSender.Config() {
		WebhookUrl = webhookUrl
	}));
}

DateTime lastSend = DateTime.MinValue;
string? cooldownEnvvar = Environment.GetEnvironmentVariable("COOLDOWN_SECONDS");
TimeSpan cooldown = cooldownEnvvar == null ? TimeSpan.FromSeconds(60) : TimeSpan.FromSeconds(int.Parse(cooldownEnvvar));

async Task<List<Photobox>?> GetPhotoboxesForVehicle(Uri vehicleUri) {
	var html = new HtmlDocument();
	using (HttpResponseMessage response = await http.GetAsync(Path.Combine(vehicleUri.ToString(), "foto"))) {
		response.EnsureSuccessStatusCode();
		html.Load(await response.Content.ReadAsStreamAsync());
	}

	var photoboxes = new List<Photobox>();

	Func<HtmlNode, Photobox> GetPhotoboxSelector(PhotoType type) =>
		node => new Photobox(
			new Uri(http.BaseAddress, node.SelectSingleNode("figure/div/a").GetAttributeValue("href", null)).ToString(),
			new Uri(http.BaseAddress, node.SelectSingleNode("figure/div/a/img").GetAttributeValue("src", null)).ToString(),
			node.SelectSingleNode("figure/figcaption/div/div[2]/strong/a[2]").GetAttributeValue("href", null).Substring("/materieel/".Length),
			node.SelectSingleNode("figure/figcaption/div[2]/div/strong/a").InnerText,
			node.SelectSingleNode("figure/figcaption/div/div[2]/strong/a[1]").InnerText,
			type,
			node.SelectSingleNode("figure/figcaption/div[3]/div").InnerText,
			node.SelectSingleNode("figure/figcaption/div[4]/div/strong/a").InnerText
		);

	var typeDict = new Dictionary<PhotoType, string>() {
		{PhotoType.General, "Algemeen"},
		{PhotoType.Interior, "Interieur"},
		{PhotoType.Detail, "Detail"},
		{PhotoType.Cabin, "Cabine"},
		{PhotoType.EngineRoom, "Motorruimte"},
	};

	foreach ((PhotoType type, string? headerTitle) in typeDict) {
		Func<HtmlNode, Photobox> selector = GetPhotoboxSelector(type);
		HtmlNodeCollection? nodes = html.DocumentNode.SelectNodes($"/html/body/div[@class='container']/a[@name='{headerTitle}']/following-sibling::div[1]/div");
		if (nodes != null) {
			photoboxes.AddRange(nodes.Select(selector).Where(photobox => !blockedPhotographers.Contains(photobox.Photographer)));
		}
	}

	if (photoboxes.Count == 0) {
		return null;
	} else {
		return photoboxes;
	}
}

async Task LookupTrainPicsAndSend(DiscordMessage message, string[] numbers) {
	try {
		foreach (string number in numbers) {
			string targetUri = $"?q={Uri.EscapeDataString(number)}&q2=";
			IList<Photobox>? photoboxes = null;

			using (HttpResponseMessage response = await http.GetAsync(targetUri, HttpCompletionOption.ResponseHeadersRead)) {
				if (response.StatusCode == HttpStatusCode.Found) {
					photoboxes = await GetPhotoboxesForVehicle(response.Headers.Location!);
				} else if (response.StatusCode == HttpStatusCode.OK) {
					var html = new HtmlDocument();
					html.Load(await response.Content.ReadAsStreamAsync());
					HtmlNode? headerNode = html.DocumentNode.SelectSingleNode("/html/body/div[1]/h2");
					if (headerNode != null && headerNode.InnerText == "Zoekresultaat") {
						IList<HtmlNode> resultRows = html.DocumentNode.SelectNodes("/html/body/div[1]/div/div/div/a");
						var exactMatchRegex = new Regex(@$"\b{number}\b");
						IList<HtmlNode> exactMatches = resultRows.Where(row => exactMatchRegex.IsMatch(row.InnerText)).ToList();

						IList<HtmlNode> candidates;
						if (exactMatches.Count == 0) {
							candidates = resultRows;
						} else {
							candidates = exactMatches;
						}

						candidates.Shuffle();
						foreach (HtmlNode candidate in candidates) {
							photoboxes = await GetPhotoboxesForVehicle(new Uri(http.BaseAddress, candidate.GetAttributeValue("href", null)));
							if (photoboxes != null) {
								break;
							}
						}
					} else {
						continue;
					}
				} else {
					string errorMessage = $"Got http {response.StatusCode} when getting {targetUri}: responding to message {message.Id} ({message.JumpLink}), numbers: {string.Join(", ", numbers)}";
					Console.WriteLine(errorMessage);
					if (notifications != null) {
						await notifications.SendNotificationAsync(errorMessage, null);
					}

					continue;
				}
			}

			if (photoboxes != null) {
				Photobox chosen = photoboxes[random.Next(0, photoboxes.Count)];

				string typeName = chosen.PhotoType switch {
					PhotoType.General => "Foto",
					PhotoType.Interior => "Interieurfoto",
					PhotoType.Detail => "Detailfoto",
					PhotoType.Cabin => "Cabinefoto",
					PhotoType.EngineRoom => "Motorruimtefoto"
				};
				
				try {
					await message.CreateReactionAsync(DiscordEmoji.FromUnicode("📷"));
				} catch (UnauthorizedException) {
					return;
				}

				lastSend = DateTime.UtcNow;
				await message.RespondAsync(dmb => dmb
					.WithEmbed(new DiscordEmbedBuilder()
						.WithAuthor(chosen.Photographer, new Uri(http.BaseAddress, Path.Combine("fotos", chosen.Photographer)).ToString())
						.WithTitle($"{typeName} van {chosen.Owner} {chosen.VehicleType} {chosen.VehicleNumber}")
						.WithUrl(chosen.PageUrl)
						.WithImageUrl(chosen.ImageUrl)
						.WithFooter($"© {chosen.Photographer}, {chosen.Taken} | Geen reacties meer? Blokkeer mij")
					)
				);
				return;
			}
		}

		string? noPhotosReactionEnvvar = Environment.GetEnvironmentVariable("NO_RESULTS_EMOTE");
		if (noPhotosReactionEnvvar != null) {
			try {
				await message.CreateReactionAsync(DiscordEmoji.FromName(discord, noPhotosReactionEnvvar, true));
			} catch (UnauthorizedException) { }
		}
	} catch (Exception e) {
		Console.WriteLine(e.ToStringDemystified());
		if (notifications != null) {
			await notifications.SendNotificationAsync($"Error responding to message {message.Id} ({message.JumpLink}), numbers: {string.Join(", ", numbers)}", e.Demystify());
		}
	}
}

var regex = new Regex(@"(?:^|[^/@#\w\n!:])(?<number>[ \d]{3,})(?:$|[^/@#\w\n:])");
discord.MessageCreated += (unused, args) => {
	if (!args.Author.IsBot) {
		MatchCollection matches = regex.Matches(args.Message.Content);
		if (matches.Count > 0) {
			if (DateTime.UtcNow - lastSend > cooldown) {
				_ = LookupTrainPicsAndSend(args.Message, matches.Select(match => match.Value).Distinct().ToArray());
			} else {
				try {
					return args.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("⏲️"));
				} catch (UnauthorizedException) { }
			}
		}
	}

	return Task.CompletedTask;
};

await discord.ConnectAsync();
await Task.Delay(-1);
