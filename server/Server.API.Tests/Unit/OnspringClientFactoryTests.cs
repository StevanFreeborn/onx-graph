namespace Server.API.Tests.Unit;

public class OnspringClientFactoryTests
{
  [Fact]
  public void CreateClient_WithApiKey_ReturnsOnspringClient()
  {
    var httpClientFactory = new Mock<IHttpClientFactory>();
    var httpClient = new HttpClient() { BaseAddress = new Uri("https://test.com") };
    httpClientFactory
      .Setup(f => f.CreateClient(OnspringClientFactory.HttpClientName))
      .Returns(httpClient);

    var factory = new OnspringClientFactory(httpClientFactory.Object);
    var apiKey = "testApiKey";

    var client = factory.CreateClient(apiKey);

    client.Should().NotBeNull();
    client.Should().BeOfType<OnspringClient>();
  }
}