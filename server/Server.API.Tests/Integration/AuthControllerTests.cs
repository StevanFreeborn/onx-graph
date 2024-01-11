using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Server.API.Tests.Data;

namespace Server.API.Tests.Integration;

public class AuthControllerTests(TestServerFactory serverFactory) : IntegrationTest(serverFactory), IDisposable
{
  public void Dispose()
  {
    context.Users.DeleteMany(_ => true);
    GC.SuppressFinalize(this);
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenValidEmailAndPassword_ItShouldReturn201StatusCodeWithRegisteredUsersId()
  {
    var newUser = FakeDataFactory.TestUser.Generate();

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", newUser);

    registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

    registerResponseBody.Should().NotBeNull();
    registerResponseBody?.Id.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenInvalidPassword_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var newUser = FakeDataFactory.TestUser.Generate();
    newUser.Password = "invalid_password";

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", newUser);

    registerResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    registerResponseBody.Should().NotBeNull();
    registerResponseBody?.Errors.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenInvalidEmail_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var newUser = FakeDataFactory.TestUser.Generate();
    newUser.Email = "invalid_email";

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", newUser);

    registerResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    registerResponseBody.Should().NotBeNull();
    registerResponseBody?.Errors.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenEmailForExistingUser_ItShouldReturn409StatusCodeWithProblemDetails()
  {
    var alreadyExistingUser = FakeDataFactory.TestUser.Generate();

    await context.Users.InsertOneAsync(alreadyExistingUser);

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