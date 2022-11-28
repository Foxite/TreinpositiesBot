using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace TreinpositiesBot;

public class TreinpositiesPhotoSource : PhotoSource {
	private readonly HttpClient m_Http;
	private readonly string[] m_BlockedPhotographers;
	private readonly Random m_Random;
	private readonly Regex m_TrainNumberRegex;

	public TreinpositiesPhotoSource() {
		// TODO DI
		m_BlockedPhotographers = (Environment.GetEnvironmentVariable("BLOCKED_PHOTOGRAPHERS") ?? "").Split(";");

		m_Random = new Random();
		
		// TODO find a way to configure redirects per request.
		m_Http = new HttpClient(new HttpClientHandler() {
			AllowAutoRedirect = false
		});
		
		m_Http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TreinpositiesBot", "0.2"));
		m_Http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Foxite/TreinpositiesBot)"));
		m_Http.BaseAddress = new Uri("https://treinposities.nl/");
		
		m_TrainNumberRegex = new Regex(@"(?:^|\s)(?<number>(?: *\d *){3,})(?:$|\s)");
	}
	
	public override IReadOnlyCollection<string> ExtractIds(string message) {
		MatchCollection matches = m_TrainNumberRegex.Matches(message);
		return matches.Select(match => match.Groups["number"].Value.Trim()).Distinct().ToArray();
	}

	public async override Task<Photobox?> GetPhoto(IReadOnlyCollection<string> ids) {
		foreach (string number in ids) {
			string targetUri = $"?q={Uri.EscapeDataString(number)}&q2=";
			IList<Photobox>? photoboxes = null;

			using (HttpResponseMessage response = await m_Http.GetAsync(targetUri, HttpCompletionOption.ResponseHeadersRead)) {
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
							photoboxes = await GetPhotoboxesForVehicle(new Uri(m_Http.BaseAddress, candidatePhotosUrl));
							if (photoboxes != null) {
								break;
							}
						}
					} else {
						continue;
					}
				} else {
					// TODO logger
					string errorMessage = $"Got http {response.StatusCode} when getting {targetUri} based on id {number}, all numbers: {string.Join(", ", ids)}";
					Console.WriteLine(errorMessage);

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
		using (HttpResponseMessage response = await m_Http.GetAsync(Path.Combine(vehicleUri.ToString(), "foto"))) {
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
					new Uri(m_Http.BaseAddress, pageUrl.GetAttributeValue("href", null)).ToString(),
					new Uri(m_Http.BaseAddress, imageUrl.GetAttributeValue("src", null)).ToString(),
					ownerString.Trim(),
					GetText(vehicleType),
					GetText(vehicleNumber),
					type,
					GetText(taken),
					photographerText,
					new Uri(m_Http.BaseAddress, Path.Combine("fotos", photographerText.Replace(' ', '_'))).ToString()
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
				photoboxes.AddRange(nodes.Select(selector).Where(photobox => !m_BlockedPhotographers.Contains(photobox.Photographer)));
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
}
