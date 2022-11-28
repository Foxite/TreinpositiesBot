using System.Drawing;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TreinpositiesBot; 

public class PlanespottersPhotoSource : PhotoSource {
	private readonly Regex m_PlaneRegistrationRegex;
	private readonly string m_AuthToken;
	private readonly HttpClient m_Http;
	private readonly Random m_Random;

	public PlanespottersPhotoSource() {
		m_PlaneRegistrationRegex = new Regex(@"^[A-Z0-9]{1,3}-?[A-Z0-9]{1,7}$");

		// TODO DI
		m_AuthToken = Environment.GetEnvironmentVariable("PLANESPOTTERS_API_TOKEN")!;
		m_Http = new HttpClient();
		m_Random = new Random();
	}
	
	public override IReadOnlyCollection<string> ExtractIds(string message) {
		MatchCollection matches = m_PlaneRegistrationRegex.Matches(message);
		return matches.Select(match => match.Value).Distinct().ToArray();
	}

	public async override Task<Photobox?> GetPhoto(IReadOnlyCollection<string> ids) {
		foreach (string reg in ids) {
			HttpResponseMessage response = await m_Http.SendAsync(new HttpRequestMessage() {
				Method = HttpMethod.Get,
				RequestUri = new Uri($"https://api.planespotters.net/v1/photos/reg/{reg}?limit=10&portrait=1"),
				Content = {
					Headers = {
						{ "x-auth-token", m_AuthToken }
					}
				}
			});

			response.EnsureSuccessStatusCode();

			string json = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeAnonymousType(json, new {Photos = Array.Empty<PlanespottersPhoto>()});
			if (result != null && result.Photos.Length > 0) {
				PlanespottersPhoto chosenPhoto = result.Photos[m_Random.Next(0, result.Photos.Length)];

				return new Photobox(
					chosenPhoto.Link,
					chosenPhoto.ThumbnailLarge.Source,
					"TODO",
					"TODO",
					reg,
					PhotoType.General,
					"TODO",
					chosenPhoto.Photographer,
					"TODO"
				);
			}
		}

		return null;
	}

	private class PlanespottersPhoto {
		public string Id { get; set; }
		public PlanespottersThumbnail Thumbnail { get; set; }
		
		[JsonProperty("thumbnail_large")]
		public PlanespottersThumbnail ThumbnailLarge { get; set; }
		
		public string Link { get; set; }
		public string Photographer { get; set; }
	}

	private class PlanespottersThumbnail {
		[JsonProperty("src")]
		public string Source { get; set; }
		public Size Size { get; set; }
	}
}
