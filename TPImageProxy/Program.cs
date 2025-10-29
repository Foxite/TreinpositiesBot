using System.Net;
using System.Net.Http.Headers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.ConfigureHttpClientDefaults(clientBuilder => {
	clientBuilder.ConfigureHttpClient(client => {
		client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TPImageProxy", "0.1"));
		client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Foxite/TreinpositiesBot)"));
	});
});

WebApplication app = builder.Build();

app.MapGet("/{photoFilename}", ProxyImage);

app.Run();
return;

async Task ProxyImage(HttpContext context, string photoFilename) {
	if (photoFilename.StartsWith("favicon")) {
		context.Response.StatusCode = (int) HttpStatusCode.NotFound;
		return;
	}

	if (photoFilename.Contains("..") || photoFilename.Contains('/')) {
		context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
		return;
	}
	
	var httpClient = context.RequestServices.GetRequiredService<HttpClient>();

	int lastUnderscore = photoFilename.LastIndexOf('_');
	if (lastUnderscore == -1) {
		context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
		return;
	}
	string photoHash = photoFilename[(lastUnderscore + 1)..];

	var uriBuilder = new UriBuilder() {
		Scheme = "https",
		Host = "treinposities.nl",
		Path = $"/tn/{photoHash[0..2]}/{photoHash[2..4]}/{photoHash[4..6]}/{photoFilename}",
	};
	HttpResponseMessage result = await httpClient.GetAsync(uriBuilder.Uri);

	result.EnsureSuccessStatusCode();
	if (!result.Content.Headers.ContentType!.MediaType!.StartsWith("image/")) {
		context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
		return;
	}

	context.Response.StatusCode = (int) HttpStatusCode.OK;
	context.Response.Headers.ContentType = result.Content.Headers.ContentType!.MediaType;
	context.Response.Headers.AccessControlAllowOrigin = "*";
	context.Response.Headers.CacheControl = "max-age=14400";

	await result.Content.CopyToAsync(context.Response.Body);
}
