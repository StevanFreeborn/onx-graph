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

  [Fact]
  public async Task AddGraph_WhenCalledWithoutName_ItShouldReturnBadRequest()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.PostAsJsonAsync("/graphs/add", new { apiKey = "API Key" });

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var responseBody = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    responseBody!.Errors["Name"].Should().Contain("'Name' must not be empty.");
  }

  [Fact]
  public async Task AddGraph_WhenCalledWithoutApiKey_ItShouldReturnBadRequest()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.PostAsJsonAsync("/graphs/add", new { name = "Test Graph" });

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var responseBody = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    responseBody!.Errors["ApiKey"].Should().Contain("'Api Key' must not be empty.");
  }
}