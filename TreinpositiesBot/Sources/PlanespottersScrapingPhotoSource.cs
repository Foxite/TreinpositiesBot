using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Foxite.Common;

namespace TreinpositiesBot; 

public class PlanespottersScrapingPhotoSource : PhotoSource {
	public static readonly Regex PlaneRegistrationRegex = new Regex(@"(?!^[0-9]+$)^[A-Z0-9]{1,3}-?[A-Z0-9]{2,7}$");
	
	private readonly Uri m_BaseUrl;
	private readonly Random m_Random;

	public override string Name => "Planespotters";

	public PlanespottersScrapingPhotoSource(Random random) {
		m_Random = random;
		m_BaseUrl = new Uri("https://www.planespotters.net/");
	}

	public override IReadOnlyCollection<string> ExtractIds(string message) {
		MatchCollection matches = PlaneRegistrationRegex.Matches(message);
		return matches.Select(match => match.Value).Distinct().ToArray();
	}
	
	public async override Task<Photobox?> GetPhoto(IReadOnlyCollection<string> ids) {
		// Can't reuse this one, unless we find a way to reset its cookies.
		// Making a request to PS causes a cookie to be set, which causes further calls to this function to fail.
		using var http = new HttpClient();
		http.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
		http.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("(X11; Linux x86_64; rv:107.0)"));
		http.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Gecko/20100101"));
		http.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Firefox/107.0"));
		http.DefaultRequestHeaders.Add("Cookie", "ps_sessid=n6CFmiPB6w9Ktfcci4WUB1OLGf");

		foreach (string reg in ids) {
			HtmlDocument document;
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Get,
				RequestUri = new Uri(m_BaseUrl, $"/photos/reg/{Uri.EscapeDataString(reg)}"),
				Content = new StringContent("") {
					Headers = {
						{ "X-Requested-With", "XMLHttpRequest" },
					}
				}
			};
			
			request.Headers.Referrer = new Uri("https://www.planespotters.net/photos");
			
			using (HttpResponseMessage response = await http.SendAsync(request)) {
				document = new HtmlDocument();
				document.Load(await response.Content.ReadAsStreamAsync());
			}
			
			IList<HtmlNode> photoElements = document.DocumentNode.QuerySelectorAll(".photo_card");
			if (photoElements.Count == 0) {
				continue;
			}

			HtmlNode chosen = photoElements[m_Random.Next(0, photoElements.Count)];

			string attributeValue = chosen.GetAttributeValue("href", null);
			return await GetPhotoboxForPhotoUrl(http, new Uri(m_BaseUrl, attributeValue));
		}

		return null;
	}

	private async Task<Photobox> GetPhotoboxForPhotoUrl(HttpClient http, Uri url) {
		HtmlDocument document;
		using (HttpResponseMessage response = await http.GetAsync(url)) {
			document = new HtmlDocument();
			document.Load(await response.Content.ReadAsStreamAsync());
		}

		string GetProperty(params string[] keys) {
			return document.DocumentNode
				.SelectSingleNode("/html/body/main/div[2]/div[1]/div[2]/div[1]")
				.QuerySelectorAll(".photo_data__heading")
				.First(property => keys.Any(key => property.InnerText.Trim().ToLower() == key))
				.ParentNode
				.QuerySelectorAll(".photo_data__link")
				.Select(node => Util.GetText(node))
				.Join(" ")
				.Trim();
		}

		HtmlNode authorNode = document.DocumentNode.SelectSingleNode("/html/body/main/div[2]/div[1]/div[2]/div[2]/div/div[2]/a[1]");
		HtmlNode imageNode = document.DocumentNode.QuerySelector(".photo_large__photo_img");
		string imageUrl = imageNode.GetAttributeValue("src", null);
		return new Photobox(
			url.ToString(),
			imageUrl,
			GetProperty("airline"),
			GetProperty("aircraft", "company"),
			GetProperty("reg"),
			PhotoType.General,
			GetProperty("date"),
			authorNode.InnerText,
			new Uri(m_BaseUrl, authorNode.GetAttributeValue("href", null)).ToString()
		);
	}
}
