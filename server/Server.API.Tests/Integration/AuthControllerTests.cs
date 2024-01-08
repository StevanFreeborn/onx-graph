using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

namespace Server.API.Tests.Integration;

public class AuthControllerTests : IClassFixture<TestServerFactory>, IDisposable
{
  private readonly MongoDbContext _context;
  private readonly HttpClient _client;

  public AuthControllerTests(TestServerFactory serverFactory)
  {
    _context = serverFactory.Services.GetRequiredService<MongoDbContext>();
    _client = serverFactory.CreateClient();
  }

  public void Dispose()
  {
    _context.Users.DeleteMany(_ => true);
    GC.SuppressFinalize(this);
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenValidEmailAndPassword_ItShouldReturn201StatusCodeWithRegisteredUsersId()
  {
    var newUser = new
    {
      email = "test@test.com",
      password = "@Password1",
    };

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", newUser);

    registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

    registerResponseBody.Should().NotBeNull();
    registerResponseBody?.Id.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenInvalidPassword_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var newUser = new
    {
      email = "test@test",
      password = "password",
    };

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", newUser);

    registerResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    registerResponseBody.Should().NotBeNull();
    registerResponseBody?.Errors.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenInvalidEmail_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var newUser = new
    {
      email = "test",
      password = "@Password1",
    };

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", newUser);

    registerResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    registerResponseBody.Should().NotBeNull();
    registerResponseBody?.Errors.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenEmailForExistingUser_ItShouldReturn409StatusCodeWithProblemDetails()
  {
    var alreadyExistingUser = new User
    {
      Email = "test@test.com",
      Password = "@Password1",
    };

    await _context.Users.InsertOneAsync(alreadyExistingUser);

    var newUser = new
    {
      email = alreadyExistingUser.Email,
      password = alreadyExistingUser.Password,
    };

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", newUser);

    registerResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    registerResponseBody.Should().NotBeNull();
  }
}