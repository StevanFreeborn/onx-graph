namespace Server.API.Tests.Integration;

public class AuthControllerTests(TestServerFactory serverFactory) : IntegrationTest(serverFactory), IDisposable
{
  public void Dispose()
  {
    context.Users.DeleteMany(_ => true);
    context.Tokens.DeleteMany(_ => true);
    GC.SuppressFinalize(this);
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenValidEmailAndPassword_ItShouldReturn201StatusCodeWithRegisteredUsersId()
  {
    var (password, user) = FakeDataFactory.TestUser.Generate();

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", new
    {
      email = user.Email,
      password
    });

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

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", new
    {
      email = user.Email,
      password = "invalid_password",
    });

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
    var (password, _) = FakeDataFactory.TestUser.Generate();

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", new
    {
      email = "invalid_email",
      password
    });

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

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", new
    {
      email = alreadyExistingUser.Email,
      password = userPassword,
    });

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

  [Fact]
  public async Task Login_WhenCalledAndGivenInvalidEmail_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var (userPassword, existingUser) = FakeDataFactory.TestUser.Generate();

    await context.Users.InsertOneAsync(existingUser);

    var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
    {
      email = "invalid_email",
      password = userPassword,
    });

    loginResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.BadRequest);

    var loginResponseBody = await loginResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    loginResponseBody
      .Should()
      .NotBeNull();

    loginResponseBody?.Errors
      .Should()
      .NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Login_WhenCalledAndGivenIncorrectEmail_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var (userPassword, existingUser) = FakeDataFactory.TestUser.Generate();

    await context.Users.InsertOneAsync(existingUser);

    var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
    {
      email = "incorrect@test.com",
      password = userPassword,
    });

    loginResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Unauthorized);

    var loginResponseBody = await loginResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    loginResponseBody
      .Should()
      .NotBeNull();

    loginResponseBody?.Title
      .Should()
      .Be("Login failed");

    loginResponseBody?.Detail
      .Should()
      .Be("Unable to login user. See errors for details.");

    loginResponseBody?.Extensions
      .Should()
      .ContainKey("Errors");
  }

  [Fact]
  public async Task Login_WhenCalledAndGivenIncorrectPassword_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();

    await context.Users.InsertOneAsync(existingUser);

    var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
    {
      email = existingUser.Email,
      password = "incorrect_password",
    });

    loginResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Unauthorized);

    var loginResponseBody = await loginResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    loginResponseBody
      .Should()
      .NotBeNull();

    loginResponseBody?.Title
      .Should()
      .Be("Login failed");

    loginResponseBody?.Detail
      .Should()
      .Be("Unable to login user. See errors for details.");

    loginResponseBody?.Extensions
      .Should()
      .ContainKey("Errors");
  }

  [Fact]
  public async Task Login_WhenCalledAndGivenEmailForNonExistingUser_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var (userPassword, user) = FakeDataFactory.TestUser.Generate();

    var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
    {
      email = user.Email,
      password = userPassword,
    });

    loginResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Unauthorized);

    var loginResponseBody = await loginResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    loginResponseBody
      .Should()
      .NotBeNull();

    loginResponseBody?.Title
      .Should()
      .Be("Login failed");

    loginResponseBody?.Detail
      .Should()
      .Be("Unable to login user. See errors for details.");

    loginResponseBody?.Extensions
      .Should()
      .ContainKey("Errors");
  }

  [Fact]
  public async Task Logout_WhenCalledByUnauthorizedUser_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var logoutResponse = await _client.PostAsync("/auth/logout", null);

    logoutResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Unauthorized);

    var logoutResponseBody = await logoutResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    logoutResponseBody
      .Should()
      .NotBeNull();

    logoutResponseBody?.Title
      .Should()
      .Be("Unauthorized");
  }

  [Fact]
  public async Task Logout_WhenCalledByAuthorizedUser_ItShouldReturn200StatusCodeAndRevokeRefreshToken()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var token = FakeDataFactory.RefreshToken.Generate();
    var userRefreshToken = token with { UserId = existingUser.Id };
    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, existingUser.Id))
      .Build();

    await context.Users.InsertOneAsync(existingUser);
    await context.Tokens.InsertOneAsync(userRefreshToken);

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    _client
      .DefaultRequestHeaders
      .Add("Cookie", $"onxRefreshToken={userRefreshToken.Token}");

    var logoutResponse = await _client.PostAsync("/auth/logout", null);

    logoutResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.OK);

    logoutResponse.Headers
      .Should()
      .Contain(h => h.Key == "Set-Cookie" && h.Value.Any(v => v.Contains("onxRefreshToken")));

    var revokedToken = await context.Tokens
      .Find(t => t.Id == userRefreshToken.Id)
      .FirstOrDefaultAsync();

    revokedToken
      .Should()
      .BeNull();
  }

  [Fact]
  public async Task RefreshToken_WhenCalledWithoutAccessToken_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var refreshTokenResponse = await _client.PostAsync("/auth/refresh-token", null);

    refreshTokenResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Unauthorized);

    var refreshTokenResponseBody = await refreshTokenResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    refreshTokenResponseBody
      .Should()
      .NotBeNull();

    refreshTokenResponseBody?.Title
      .Should()
      .Be("Unauthorized");
  }

  [Fact]
  public async Task RefreshToken_WhenCalledWithExpiredAccessToken_ItShouldNotReturn401StatusCodeWithProblemDetails()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, existingUser.Id))
      .WithIssuedAt(DateTime.UtcNow.AddMinutes(-(TestJwtTokenBuilder.TestJwtExpiryInMinutes + 1)))
      .Build();
    var refreshToken = FakeDataFactory.RefreshToken.Generate() with { UserId = existingUser.Id };

    await context.Tokens.InsertOneAsync(refreshToken);
    await context.Users.InsertOneAsync(existingUser);

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    _client
      .DefaultRequestHeaders
      .Add("Cookie", $"onxRefreshToken={refreshToken.Token}");

    var refreshTokenResponse = await _client.PostAsync("/auth/refresh-token", null);

    refreshTokenResponse.StatusCode
      .Should()
      .NotBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task RefreshToken_WhenCalledAndNoRefreshTokenIsPresent_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, existingUser.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var refreshTokenResponse = await _client.PostAsync("/auth/refresh-token", null);

    refreshTokenResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Unauthorized);

    var refreshTokenResponseBody = await refreshTokenResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    refreshTokenResponseBody
      .Should()
      .NotBeNull();

    refreshTokenResponseBody?.Title
      .Should()
      .Be("Unauthorized");
  }

  [Fact]
  public async Task RefreshToken_WhenCalledAndRefreshTokenDoesNotExist_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, existingUser.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    _client
      .DefaultRequestHeaders
      .Add("Cookie", $"onxRefreshToken=non_existing_token");

    var refreshTokenResponse = await _client.PostAsync("/auth/refresh-token", null);

    refreshTokenResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Unauthorized);

    var refreshTokenResponseBody = await refreshTokenResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    refreshTokenResponseBody
      .Should()
      .NotBeNull();

    refreshTokenResponseBody?.Title
      .Should()
      .Be("Unauthorized");
  }

  [Fact]
  public async Task RefreshToken_WhenCalledAndUserDoesNotExist_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var refreshToken = FakeDataFactory.RefreshToken.Generate() with { UserId = existingUser.Id };
    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, "non_existing_user_id"))
      .Build();

    await context.Tokens.InsertOneAsync(refreshToken);

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    _client
      .DefaultRequestHeaders
      .Add("Cookie", $"onxRefreshToken={refreshToken.Token}");

    var refreshTokenResponse = await _client.PostAsync("/auth/refresh-token", null);

    refreshTokenResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Unauthorized);

    var refreshTokenResponseBody = await refreshTokenResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    refreshTokenResponseBody
      .Should()
      .NotBeNull();

    refreshTokenResponseBody?.Title
      .Should()
      .Be("Unauthorized");
  }

  [Fact]
  public async Task RefreshToken_WhenRefreshTokenIsExpired_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var refreshToken = FakeDataFactory.RefreshToken.Generate() with
    {
      UserId = existingUser.Id,
      ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
    };

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, existingUser.Id))
      .Build();

    await context.Tokens.InsertOneAsync(refreshToken);
    await context.Users.InsertOneAsync(existingUser);

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    _client
      .DefaultRequestHeaders
      .Add("Cookie", $"onxRefreshToken={refreshToken.Token}");

    var refreshTokenResponse = await _client.PostAsync("/auth/refresh-token", null);

    refreshTokenResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Unauthorized);

    var refreshTokenResponseBody = await refreshTokenResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    refreshTokenResponseBody
      .Should()
      .NotBeNull();

    refreshTokenResponseBody?.Title
      .Should()
      .Be("Unauthorized");
  }

  [Fact]
  public async Task RefreshToken_WhenRefreshTokenDoesNotBelongToUser_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var refreshToken = FakeDataFactory.RefreshToken.Generate();
    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, existingUser.Id))
      .Build();

    await context.Tokens.InsertOneAsync(refreshToken);
    await context.Users.InsertOneAsync(existingUser);

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    _client
      .DefaultRequestHeaders
      .Add("Cookie", $"onxRefreshToken={refreshToken.Token}");

    var refreshTokenResponse = await _client.PostAsync("/auth/refresh-token", null);

    refreshTokenResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Unauthorized);

    var refreshTokenResponseBody = await refreshTokenResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    refreshTokenResponseBody
      .Should()
      .NotBeNull();

    refreshTokenResponseBody?.Title
      .Should()
      .Be("Unauthorized");
  }

  [Fact]
  public async Task RefreshToken_WhenCalledAndGivenValidRefreshToken_ItShouldReturn200StatusCodeWithNewAccessTokenAndRefreshToken()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var refreshToken = FakeDataFactory.RefreshToken.Generate() with { UserId = existingUser.Id };
    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, existingUser.Id))
      .Build();

    await context.Tokens.InsertOneAsync(refreshToken);
    await context.Users.InsertOneAsync(existingUser);

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    _client
      .DefaultRequestHeaders
      .Add("Cookie", $"onxRefreshToken={refreshToken.Token}");

    var refreshTokenResponse = await _client.PostAsync("/auth/refresh-token", null);

    refreshTokenResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.OK);

    refreshTokenResponse.Headers
      .Should()
      .Contain(h => h.Key == "Set-Cookie" && h.Value.Any(v => v.Contains("onxRefreshToken")));

    var refreshTokenResponseBody = await refreshTokenResponse.Content.ReadFromJsonAsync<LoginUserResponse>();

    refreshTokenResponseBody
      .Should()
      .NotBeNull();

    refreshTokenResponseBody?.AccessToken
      .Should()
      .NotBeNullOrEmpty();

    var existingRefreshToken = await context.Tokens
      .Find(t => t.Id == refreshToken.Id)
      .FirstOrDefaultAsync();

    existingRefreshToken
      .Should()
      .BeNull();
  }
}