namespace Server.API.Tests.Integration;

public class GraphsControllerTests(TestServerFactory serverFactory) : IntegrationTest(serverFactory), IDisposable
{
  public void Dispose()
  {
    GC.SuppressFinalize(this);
  }

  [Fact]
  public async Task AddGraph_WhenCalledByUnauthenticatedUser_ItShouldReturnUnauthorized()
  {
    var response = await _client.PostAsJsonAsync("/graphs/add", new { });

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }
}