using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TreinpositiesBot;

public class TreinpositiesPhotoSource : PhotoSource {
	private static readonly Regex TrainNumberRegex = new Regex(@"(?:^|\s)(?<number>(?: *\d *){3,})(?:$|\s)");
	
	private readonly IOptionsMonitor<Options> m_Options;
	private readonly HttpClient m_Http;
	private readonly Random m_Random;
	private readonly ILogger<TreinpositiesPhotoSource> m_Logger;

	public TreinpositiesPhotoSource(IOptionsMonitor<Options> options, HttpClient http, Random random, ILogger<TreinpositiesPhotoSource> logger) {
		m_Options = options;
		m_Http = http;
		m_Random = random;
		m_Logger = logger;
	}
	
	public override IReadOnlyCollection<string> ExtractIds(string message) {
		MatchCollection matches = TrainNumberRegex.Matches(message);
		return matches.Select(match => match.Groups["number"].Value.Trim()).Distinct().ToArray();
	}

	public async override Task<Photobox?> GetPhoto(IReadOnlyCollection<string> ids) {
		foreach (string number in ids) {
			string targetUri = $"?q={Uri.EscapeDataString(number)}&q2=";
			IList<Photobox>? photoboxes = null;

			using (HttpResponseMessage response = await m_Http.GetAsync(new Uri(m_Options.CurrentValue.BaseAddress, targetUri), HttpCompletionOption.ResponseHeadersRead)) {
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
						foreach (HtmlNode candidate in candidates.Take(5)) {
							string candidatePhotosUrl = candidate.GetAttributeValue("href", null);
							photoboxes = await GetPhotoboxesForVehicle(new Uri(m_Options.CurrentValue.BaseAddress, candidatePhotosUrl));
							if (photoboxes != null) {
								break;
							}
						}
					} else {
						continue;
					}
				} else {
					m_Logger.LogError("Got http {ResponseStatusCode} when getting {TargetUri} based on id {Number}, all numbers: {Ids}", response.StatusCode, targetUri, number, string.Join(", ", ids));
					continue;
				}
			}

			if (photoboxes != null) {
				return photoboxes[m_Random.Next(0, photoboxes.Count)];
			}
		}

		return null;
	}

	async Task<List<Photobox>?> GetPhotoboxesForVehicle(Uri vehicleUri) {
		var html = new HtmlDocument();
		using (HttpResponseMessage response = await m_Http.GetAsync(new Uri(m_Options.CurrentValue.BaseAddress, Path.Combine(vehicleUri.ToString(), "foto")))) {
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

				string photographerText = GetText(photographer);
				
				return new Photobox(
					new Uri(m_Options.CurrentValue.BaseAddress, pageUrl.GetAttributeValue("href", null)).ToString(),
					new Uri(m_Options.CurrentValue.BaseAddress, imageUrl.GetAttributeValue("src", null)).ToString(),
					ownerString.Trim(),
					GetText(vehicleType),
					GetText(vehicleNumber),
					type,
					GetText(taken),
					photographerText,
					new Uri(m_Options.CurrentValue.BaseAddress, Path.Combine("fotos", photographerText.Replace(' ', '_'))).ToString()
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
				photoboxes.AddRange(nodes.Select(selector).Where(photobox => !m_Options.CurrentValue.BlockedPhotographers.Contains(photobox.Photographer)));
			}
		}

		if (photoboxes.Count == 0) {
			return null;
		} else {
			return photoboxes;
		}
	}

	[return: NotNullIfNotNull("node")]
	static string? GetText(HtmlNode? node) {
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

	public class Options {
		public string[] BlockedPhotographers { get; set; } = Array.Empty<string>();
		public Uri BaseAddress { get; set; } = new Uri("https://treinposities.nl/");
	}
}
