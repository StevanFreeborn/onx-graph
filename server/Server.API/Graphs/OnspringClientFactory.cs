namespace Server.API.Graphs;

class OnspringClientFactory(IHttpClientFactory httpClientFactory) : IOnspringClientFactory
{
  public static string HttpClientName = "OnspringHttpClient";
  private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

  public IOnspringClient CreateClient(string apiKey)
  {
    var httpClient = _httpClientFactory.CreateClient(HttpClientName);
    return new OnspringClient(apiKey, httpClient);
  }
}