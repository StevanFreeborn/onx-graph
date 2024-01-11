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
    var (password, user) = FakeDataFactory.TestUser.Generate();

    var newUser = new
    {
      email = user.Email,
      password,
    };

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", newUser);

    registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

    registerResponseBody
      .Should()
      .NotBeNull();

    registerResponseBody?.Id
      .Should()
      .NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenInvalidPassword_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var newUser = new
    {
      email = user.Email,
      password = "invalid_password",
    };

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", newUser);

    registerResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    registerResponseBody
      .Should()
      .NotBeNull();

    registerResponseBody?.Errors
      .Should()
      .NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenInvalidEmail_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var (password, user) = FakeDataFactory.TestUser.Generate();

    var newUser = new
    {
      email = "invalid_email",
      password,
    };

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", newUser);

    registerResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    registerResponseBody
      .Should()
      .NotBeNull();

    registerResponseBody?.Errors
      .Should()
      .NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenEmailForExistingUser_ItShouldReturn409StatusCodeWithProblemDetails()
  {
    var (userPassword, alreadyExistingUser) = FakeDataFactory.TestUser.Generate();

    await context.Users.InsertOneAsync(alreadyExistingUser);

    var newUser = new
    {
      email = alreadyExistingUser.Email,
      password = userPassword,
    };

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", newUser);

    registerResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Conflict);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    registerResponseBody
      .Should()
      .NotBeNull();
  }

  [Fact]
  public async Task Login_WhenCalledAndGivenValidEmailAndPassword_ItShouldReturn200StatusCodeWithAccessTokenAndRefreshToken()
  {
    var (userPassword, existingUser) = FakeDataFactory.TestUser.Generate();

    await context.Users.InsertOneAsync(existingUser);

    var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
    {
      email = existingUser.Email,
      password = userPassword,
    });

    loginResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.OK);

    loginResponse.Headers
      .Should()
      .Contain(h => h.Key == "Set-Cookie" && h.Value.Any(v => v.Contains("onxRefreshToken")));

    var loginResponseBody = await loginResponse.Content.ReadFromJsonAsync<LoginUserResponse>();

    loginResponseBody
      .Should()
      .NotBeNull();

    loginResponseBody?.AccessToken
      .Should()
      .NotBeNullOrEmpty();
  }
}