namespace Server.API.Tests.Integration;

public class ErrorMiddlewareTests(TestServerFactory serverFactory) : IntegrationTest(serverFactory)
{
  [Fact]
  public async Task ErrorMiddleware_WhenUnhandledExceptionOccurs_ItShouldReturn500StatusCodeWithProblemDetails()
  {
    var mockUserService = new Mock<IUserService>();

    mockUserService
      .Setup(service => service.GetUserByIdAsync(It.IsAny<string>()))
      .ThrowsAsync(new Exception("An error occurred"));

    var client = _factory
      .WithWebHostBuilder(
        builder => builder.ConfigureTestServices(
          services => services.AddSingleton(mockUserService.Object)
        )
      ).CreateClient();

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, "Test User"))
      .Build();

    client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var requestBody = new { name = "Test Graph", apiKey = "API Key" };

    var response = await client.PostAsJsonAsync("/graphs/add", requestBody);

    response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
  }
}