using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace TreinpositiesBot;

public class JetphotosPhotoSource : PhotoSource {
	public override string Name => "Jetphotos.net";
	
	private readonly IOptionsMonitor<Options> m_Options;
	private readonly HttpClient m_Http;
	private readonly ILogger<JetphotosPhotoSource> m_Logger;
	private readonly Random m_Random;

	public JetphotosPhotoSource(IOptionsMonitor<Options> options, HttpClient http, ILogger<JetphotosPhotoSource> logger, Random random) {
		m_Options = options;
		m_Http = http;
		m_Logger = logger;
		m_Random = random;
	}

	public override IReadOnlyCollection<string> ExtractIds(string message) {
		MatchCollection matches = PlanespottersScrapingPhotoSource.PlaneRegistrationRegex.Matches(message);
		return matches.Select(match => match.Value).Distinct().ToArray();
	}
	
	public async override Task<Photobox?> GetPhoto(IReadOnlyCollection<string> ids) {
		foreach (var id in ids) {
			HtmlDocument photosPage = await FlaresolverrRequest($"https://www.jetphotos.com/photo/keyword/{id}");
			List<int>? resultIds = photosPage.QuerySelectorAll(".result")?.Select(div => div.GetAttributeValue("data-photo", -1))?.Where(photoId => photoId != -1)?.ToList();
			if (resultIds == null || resultIds.Count == 0) {
				return null;
			}

			var chosenId = resultIds[m_Random.Next(0, resultIds.Count)];
			var photoPageUrl = $"https://www.jetphotos.com/photo/{chosenId}";
			HtmlDocument photoPage = await FlaresolverrRequest(photoPageUrl);

			string? GetField(string label) {
				var headerText3s = photoPage.QuerySelectorAll("h3");
				var fieldHeader = headerText3s.FirstOrDefault(node => node.InnerText == label);						

				return fieldHeader?.NextSiblingElement()?.InnerText;
			}

			var photographerUrlNode = photoPage
				.QuerySelectorAll("a.link")
				.Where(node => {
					var href = node.GetAttributeValue("href", "");
					return href.StartsWith("/photographer/") && !href.EndsWith("/photos");
				})
				.ElementAt(1); // stupid heuristic

			HtmlNode previousSiblingElement = photographerUrlNode.ParentNode.PreviousSiblingElement();
				return new Photobox(	
				photoPageUrl,	
				photoPage.QuerySelector("#large-photo-wrapper img").GetAttributeValue("src", ""),
				photoPage.GetElementbyId("airline").GetAttributeValue("value", null),
				photoPage.GetElementbyId("aircraft").GetAttributeValue("value", null),
				photoPage.GetElementbyId("reg").GetAttributeValue("value", null),		
				PhotoType.General,
				GetField("Photo Date") ?? GetField("Uploaded") ?? "Unknown (wtf?)",
				previousSiblingElement.InnerText.Trim(),
				$"https://www.jetphotos.com/{photographerUrlNode.GetAttributeValue("href", null)!}"
			);
		}

		return null;
	}

	private async Task<HtmlDocument> FlaresolverrRequest(string url) {
		HttpResponseMessage response = await m_Http.SendAsync(new HttpRequestMessage() {
			RequestUri = m_Options.CurrentValue.FlaresolverrUrl,
			Method = HttpMethod.Post,
			Content = new StringContent(JsonConvert.SerializeObject(new {
				cmd = "request.get",
				url = url,
				maxTimeout = 60_000,
			})) {
				Headers = {
					ContentType = new MediaTypeHeaderValue("application/json"),
				},
			},
		});

		var result = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new {
			status = "ok",
			message = "Challenge not detected!",
			solution = new {
				status = 200,
				response = "html",
			},
		});

		if (!response.IsSuccessStatusCode) {
			throw new Exception($"Non-successful response from Flaresolverr: {response.StatusCode}, {result?.status}, {result?.message}");
		}

		if (result == null) {
			throw new Exception($"Unable to deserialize Flaresolverr response: {await response.Content.ReadAsStringAsync()}");
			return null;
		}

		if (result.solution.status != 200) {
			throw new Exception($"Non-successful response from Jetphotos.net: {result.solution.status}");
		}

		var html = new HtmlDocument();
		html.LoadHtml(result.solution.response);
		return html;
	}
	
	public class Options {
		public Uri FlaresolverrUrl { get; set; }
	}
}
