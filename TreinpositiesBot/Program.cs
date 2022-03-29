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
			new ProductInfoHeaderValue("TreinpositiesBot", "0.1"),
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
async Task LookupTrainPicsAndSend(DiscordMessage message, string[] numbers) {
	try {
		foreach (string number in numbers) {
			Uri vehicleUrl;
			string targetUri = $"?q={Uri.EscapeDataString(number)}&q2=";

			using (HttpResponseMessage response = await http.GetAsync(targetUri, HttpCompletionOption.ResponseHeadersRead)) {
				string content = await response.Content.ReadAsStringAsync();
				if (response.StatusCode == HttpStatusCode.Found) {
					vehicleUrl = response.Headers.Location!;
				} else {
					continue;
				}
			}

			using (HttpResponseMessage response = await http.GetAsync(Path.Combine(vehicleUrl.ToString(), "foto"))) {
				response.EnsureSuccessStatusCode();
				var html = new HtmlDocument();
				html.Load(await response.Content.ReadAsStreamAsync());
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
					{ PhotoType.General, "Algemeen" },
					{ PhotoType.Interior, "Interieur" },
					{ PhotoType.Detail, "Detail" },
				};
				foreach ((PhotoType type, string? headerTitle) in typeDict) {
					Func<HtmlNode, Photobox> selector = GetPhotoboxSelector(type);
					HtmlNodeCollection? nodes = html.DocumentNode.SelectNodes($"/html/body/div[@class='container']/a[@name='{headerTitle}']/following-sibling::div[1]/div");
					if (nodes != null) {
						photoboxes.AddRange(nodes.Select(selector).Where(photobox => !blockedPhotographers.Contains(photobox.Photographer)));
					}
				}

				if (photoboxes.Count > 0) {
					Photobox chosen = photoboxes[random.Next(0, photoboxes.Count)];

					string typeName = chosen.PhotoType switch {
						PhotoType.General => "Foto",
						PhotoType.Interior => "Interieurfoto",
						PhotoType.Detail => "Detailfoto"
					};
					
					try {
						await message.CreateReactionAsync(DiscordEmoji.FromUnicode("📷"));
					} catch (UnauthorizedException) {
						return;
					}
		
					await message.DeleteOwnReactionAsync(DiscordEmoji.FromUnicode("📷"));
					
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
		}
	} catch (Exception e) {
		Console.WriteLine(e.ToStringDemystified());
		if (notifications != null) {
			await notifications.SendNotificationAsync($"Error responding to message {message.Id} ({message.JumpLink}), numbers: {string.Join(", ", numbers)}", e.Demystify());
		}
	}
}

var regex = new Regex(@"(?:^|[^/@#\w])(?<number>[ \d]{3,})(?:$|[^/@#\w])");
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
