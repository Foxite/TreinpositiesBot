using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

var lastSendPerUser = new ConcurrentDictionary<ulong, DateTime>();
string? cooldownEnvvar = Environment.GetEnvironmentVariable("COOLDOWN_SECONDS");
TimeSpan cooldown = cooldownEnvvar == null ? TimeSpan.FromSeconds(60) : TimeSpan.FromSeconds(int.Parse(cooldownEnvvar));

[return: NotNullIfNotNull("node")]
string? GetText(HtmlNode? node) {
	if (node == null) {
		return null;
	}

	if (node.NodeType == HtmlNodeType.Text) {
		return HtmlEntity.DeEntitize(node.InnerText).Trim();
	}

	string ret = "";
	foreach (HtmlNode childNode in node.ChildNodes) {
		ret += GetText(childNode);
	}

	return ret;
}

async Task<List<Photobox>?> GetPhotoboxesForVehicle(Uri vehicleUri) {
	var html = new HtmlDocument();
	using (HttpResponseMessage response = await http.GetAsync(Path.Combine(vehicleUri.ToString(), "foto"))) {
		response.EnsureSuccessStatusCode();
		html.Load(await response.Content.ReadAsStreamAsync());
	}

	var photoboxes = new List<Photobox>();

	Func<HtmlNode, Photobox> GetPhotoboxSelector(PhotoType type) =>
		node => {
			HtmlNode pageUrl = node.SelectSingleNode("figure/div/a");
			HtmlNode imageUrl = node.SelectSingleNode("figure/div/a/img");
			HtmlNode? owner = node.SelectSingleNode("figure/figcaption/div/div[2]/strong/a[2]");
			HtmlNode? vehicleType = node.SelectSingleNode("figure/figcaption/div[2]/div/strong/a");
			HtmlNode vehicleNumber = node.SelectSingleNode("figure/figcaption/div/div[2]/strong/a[1]");
			HtmlNode taken = node.SelectSingleNode("figure/figcaption/div[3]/div");
			HtmlNode photographer = node.SelectSingleNode($"figure/figcaption/div[{(vehicleType == null ? 3 : 4)}]/div/strong/a");

			string ownerString = owner.GetAttributeValue("href", null).Substring("/materieel/".Length);
			if (string.IsNullOrWhiteSpace(ownerString)) {
				ownerString = HtmlEntity.DeEntitize(owner.InnerText);
			}
			
			return new Photobox(
				new Uri(http.BaseAddress, pageUrl.GetAttributeValue("href", null)).ToString(),
				new Uri(http.BaseAddress, imageUrl.GetAttributeValue("src", null)).ToString(),
				ownerString.Trim(),
				GetText(vehicleType),
				GetText(vehicleNumber),
				type,
				GetText(taken),
				GetText(photographer)
			);
		};

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
	Photobox? chosenPhotobox = null; 
	string? vehicle = null;
	try {
		foreach (string number in numbers) {
			string targetUri = $"?q={Uri.EscapeDataString(number)}&q2=";
			IList<Photobox>? photoboxes = null;

			using (HttpResponseMessage response = await http.GetAsync(targetUri, HttpCompletionOption.ResponseHeadersRead)) {
				if (response.StatusCode == HttpStatusCode.Found) {
					vehicle = response.Headers.Location!.ToString();
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
						foreach (HtmlNode candidate in candidates.Take(5)) {
							string candidatePhotosUrl = candidate.GetAttributeValue("href", null);
							vehicle = candidatePhotosUrl;
							photoboxes = await GetPhotoboxesForVehicle(new Uri(http.BaseAddress, candidatePhotosUrl));
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
				chosenPhotobox = photoboxes[random.Next(0, photoboxes.Count)];
				
				string typeName = chosenPhotobox.PhotoType switch {
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

				lastSendPerUser[message.Author.Id] = DateTime.UtcNow;
				await message.RespondAsync(dmb => dmb
					.WithEmbed(new DiscordEmbedBuilder()
						.WithAuthor(chosenPhotobox.Photographer, new Uri(http.BaseAddress, Path.Combine("fotos", chosenPhotobox.Photographer.Replace(' ', '_'))).ToString())
						.WithTitle($"{typeName} van {chosenPhotobox.Identity}")
						.WithUrl(chosenPhotobox.PageUrl)
						.WithImageUrl(chosenPhotobox.ImageUrl)
						.WithFooter($"© {chosenPhotobox.Photographer}, {chosenPhotobox.Taken} | Geen reacties meer? Blokkeer mij")
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
		string report = $"Error responding to message {message.Id} ({message.JumpLink}), numbers: {string.Join(", ", numbers)}; vehicle url: {(vehicle ?? "null")}; photo url: ${(chosenPhotobox?.PageUrl ?? "null")}";
		Console.WriteLine(report);
		Console.WriteLine(e.ToStringDemystified());
		if (notifications != null) {
			await notifications.SendNotificationAsync(report, e.Demystify());
		}
	}
}

var regex = new Regex(@"(?:^|\s)(?<number>(?: *\d *){3,})(?:$|\s)");
discord.MessageCreated += (unused, args) => {
	if (args.Guild != null) {
		Permissions perms = args.Channel.PermissionsFor(args.Guild.CurrentMember);
		if (!perms.HasFlag(args.Channel.IsThread ? Permissions.SendMessagesInThreads : Permissions.SendMessages)) {
			return Task.CompletedTask;
		}
	}

	if (args.Author.IsBot) {
		return Task.CompletedTask;
	}

	MatchCollection matches = regex.Matches(args.Message.Content);
	if (matches.Count > 0) {
		if (!lastSendPerUser.TryGetValue(args.Author.Id, out DateTime lastSend) || DateTime.UtcNow - lastSend > cooldown) {
			_ = LookupTrainPicsAndSend(args.Message, matches.Select(match => match.Groups["number"].Value.Trim()).Distinct().ToArray());
		} else {
			try {
				return args.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("⏲️"));
			} catch (UnauthorizedException) { }
		}
	}

	return Task.CompletedTask;
};

await discord.ConnectAsync();
await Task.Delay(-1);
