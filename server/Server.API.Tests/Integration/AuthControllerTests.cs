namespace Server.API.Tests.Integration;

public partial class AuthControllerTests(TestServerFactory serverFactory) : IntegrationTest(serverFactory)
{
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

    registerResponseBody!.Id
      .Should()
      .NotBeNullOrEmpty();

    var createdUser = await Context.Users
      .Find(u => u.Id == registerResponseBody.Id)
      .SingleOrDefaultAsync();

    createdUser.Should().NotBeNull();
    createdUser.Password.Should().NotBe(password);
    createdUser.EncryptionKey.Should().NotBeNullOrEmpty();
  }

  [GeneratedRegex(@"\/masses\/verify-account\?t=[a-zA-Z0-9]+")]
  private static partial Regex VerifyAccountLinkRegex();

  [Fact]
  public async Task Register_WhenCalledAndGivenValidEmailAndPassword_ItShouldSendVerificationEmail()
  {
    var (password, _) = FakeDataFactory.TestUser.Generate();
    var testEmail = $"test.user.{Guid.NewGuid()}@test.com";
    var emailParts = testEmail.Split('@');
    var testMailbox = emailParts[0];
    var testDomain = emailParts[1];

    var registerResponse = await _client.PostAsJsonAsync("/auth/register", new
    {
      email = testEmail,
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

    var emailSearchResult = await _mailHogService.SearchEmailAsync(new(MailHogSearchKind.To, testEmail));
    emailSearchResult.Count.Should().Be(1);
    emailSearchResult.Items.Should().NotBeNullOrEmpty();
    emailSearchResult.Items.First().To
      .Should()
      .ContainSingle(t =>
        t.Mailbox == testMailbox && t.Domain == testDomain
      );

    var email = await _mailHogService.GetEmailAsync(emailSearchResult.Items.First().Id);
    email.Content.Body.Should().Contain("Verify Account");
    email.Content.Body.Should().MatchRegex(VerifyAccountLinkRegex());
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

    await Context.Users.InsertOneAsync(alreadyExistingUser);

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
    existingUser.IsVerified = true;

    await Context.Users.InsertOneAsync(existingUser);

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
  public async Task Login_WhenCalledAndGivenValidEmailAndPasswordButUserIsNotVerified_ItShouldReturn403StatusCodeWithProblemDetails()
  {
    var (userPassword, existingUser) = FakeDataFactory.TestUser.Generate();

    await Context.Users.InsertOneAsync(existingUser);

    var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
    {
      email = existingUser.Email,
      password = userPassword,
    });

    loginResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Forbidden);

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
  public async Task Login_WhenCalledAndGivenInvalidEmail_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var (userPassword, existingUser) = FakeDataFactory.TestUser.Generate();

    await Context.Users.InsertOneAsync(existingUser);

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

    await Context.Users.InsertOneAsync(existingUser);

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

    await Context.Users.InsertOneAsync(existingUser);

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

    await Context.Users.InsertOneAsync(existingUser);
    await Context.Tokens.InsertOneAsync(userRefreshToken);

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

    var revokedToken = await Context.Tokens
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

    await Context.Tokens.InsertOneAsync(refreshToken);
    await Context.Users.InsertOneAsync(existingUser);

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

    await Context.Tokens.InsertOneAsync(refreshToken);

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

    await Context.Tokens.InsertOneAsync(refreshToken);
    await Context.Users.InsertOneAsync(existingUser);

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

    await Context.Tokens.InsertOneAsync(refreshToken);
    await Context.Users.InsertOneAsync(existingUser);

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

    await Context.Tokens.InsertOneAsync(refreshToken);
    await Context.Users.InsertOneAsync(existingUser);

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

    var existingRefreshToken = await Context.Tokens
      .Find(t => t.Id == refreshToken.Id)
      .FirstOrDefaultAsync();

    existingRefreshToken
      .Should()
      .BeNull();
  }

  [Theory]
  [InlineData("")]
  [InlineData("invalid_email@")]
  public async Task ResendVerificationToken_WhenCalledAndGivenInvalidEmail_ItShouldReturn400StatusCodeWithValidationProblemDetails(string email)
  {
    var resendResponse = await _client.PostAsJsonAsync("/auth/resend-verification-email", new
    {
      email,
    });

    resendResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.BadRequest);

    var resendResponseBody = await resendResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    resendResponseBody
      .Should()
      .NotBeNull();

    resendResponseBody?.Errors
      .Should()
      .NotBeNullOrEmpty();
  }

  [Fact]
  public async Task ResendVerificationToken_WhenCalledAndGivenEmailForNonExistingUser_ItShouldReturn404StatusCodeWithProblemDetails()
  {
    var resendResponse = await _client.PostAsJsonAsync("/auth/resend-verification-email", new
    {
      email = "test@test.com",
    });

    resendResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.NotFound);

    var resendResponseBody = resendResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    resendResponseBody.Result
      .Should()
      .NotBeNull();

    resendResponseBody.Result?.Title
      .Should()
      .Be("Resend verification email failed");

    resendResponseBody.Result?.Detail
      .Should()
      .Be("Unable to resend verification email. See errors for details.");

    resendResponseBody.Result?.Extensions
      .Should()
      .ContainKey("Errors");
  }

  [Fact]
  public async Task ResendVerificationToken_WhenCalledAndGivenEmailForAlreadyVerifiedUser_ItShouldReturn400StatusCodeWithProblemDetails()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    existingUser.IsVerified = true;

    Context.Users.InsertOne(existingUser);

    var resendResponse = await _client.PostAsJsonAsync("/auth/resend-verification-email", new
    {
      email = existingUser.Email,
    });

    resendResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Conflict);

    var resendResponseBody = resendResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    resendResponseBody.Result
      .Should()
      .NotBeNull();

    resendResponseBody.Result?.Title
      .Should()
      .Be("Resend verification email failed");

    resendResponseBody.Result?.Detail
      .Should()
      .Be("Unable to resend verification email. See errors for details.");

    resendResponseBody.Result?.Extensions
      .Should()
      .ContainKey("Errors");
  }

  [Fact]
  public async Task ResendVerificationToken_WhenCalledAndGivenEmailForExistingUser_ItShouldReturn204StatusCodeSendEmailAndRevokeExistingVerificationTokens()
  {
    var (password, existingUser) = FakeDataFactory.TestUser.Generate();
    var existingVerificationToken = FakeDataFactory.VerificationToken.Generate() with { UserId = existingUser.Id };
    var emailParts = existingUser.Email.Split('@');
    var testMailbox = emailParts[0];
    var testDomain = emailParts[1];

    await Context.Users.InsertOneAsync(existingUser);
    await Context.Tokens.InsertOneAsync(existingVerificationToken);

    var resendResponse = await _client.PostAsJsonAsync("/auth/resend-verification-email", new
    {
      email = existingUser.Email,
    });

    resendResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

    var resendResponseBody = await resendResponse.Content.ReadAsStringAsync();

    resendResponseBody
      .Should()
      .BeEmpty();

    var newToken = await Context.Tokens
      .Find(
        t =>
          t.UserId == existingUser.Id &&
          t.TokenType == TokenType.Verification &&
          t.Token != existingVerificationToken.Token &&
          t.Revoked == false
      )
      .FirstOrDefaultAsync();

    newToken
      .Should()
      .NotBeNull();

    var revokedToken = await Context.Tokens
      .Find(t => t.Id == existingVerificationToken.Id)
      .FirstOrDefaultAsync();

    revokedToken
      .Should()
      .NotBeNull();

    revokedToken?.Revoked.Should().BeTrue();

    var emailSearchResult = await _mailHogService.SearchEmailAsync(new(MailHogSearchKind.To, existingUser.Email));
    emailSearchResult.Count.Should().Be(1);
    emailSearchResult.Items.Should().NotBeNullOrEmpty();
    emailSearchResult.Items.First().To
      .Should()
      .ContainSingle(t =>
        t.Mailbox == testMailbox && t.Domain == testDomain
      );

    var email = await _mailHogService.GetEmailAsync(emailSearchResult.Items.First().Id);
    email.Content.Body.Should().Contain("Verify Account");
    email.Content.Body.Should().MatchRegex(VerifyAccountLinkRegex());
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndNoTokenIsGiven_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var verifyResponse = await _client.PostAsJsonAsync("/auth/verify-account", new
    {
      token = "",
    });

    verifyResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.BadRequest);

    var verifyResponseBody = await verifyResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    verifyResponseBody
      .Should()
      .NotBeNull();

    verifyResponseBody?.Errors
      .Should()
      .NotBeNullOrEmpty();
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndTokenDoesNotExist_ItShouldReturn404StatusCodeWithProblemDetails()
  {
    var verifyResponse = await _client.PostAsJsonAsync("/auth/verify-account", new
    {
      token = "non_existing_token",
    });

    verifyResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.NotFound);

    var verifyResponseBody = await verifyResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    verifyResponseBody
      .Should()
      .NotBeNull();

    verifyResponseBody?.Title
      .Should()
      .Be("Verification failed");

    verifyResponseBody?.Detail
      .Should()
      .Be("Unable to verify account. See errors for details.");

    verifyResponseBody?.Extensions
      .Should()
      .ContainKey("Errors");
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndTokenIsExpired_ItShouldReturn400StatusCodeWithProblemDetails()
  {
    var verificationToken = FakeDataFactory.VerificationToken.Generate() with
    {
      ExpiresAt = DateTime.UtcNow.AddMinutes(-30),
    };

    await Context.Tokens.InsertOneAsync(verificationToken);

    var verifyResponse = await _client.PostAsJsonAsync("/auth/verify-account", new
    {
      token = verificationToken.Token,
    });

    verifyResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.BadRequest);

    var verifyResponseBody = await verifyResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    verifyResponseBody
      .Should()
      .NotBeNull();

    verifyResponseBody?.Title
      .Should()
      .Be("Verification failed");

    verifyResponseBody?.Detail
      .Should()
      .Be("Unable to verify account. See errors for details.");

    verifyResponseBody?.Extensions
      .Should()
      .ContainKey("Errors");

    var revokedToken = await Context.Tokens
      .Find(t => t.Id == verificationToken.Id)
      .FirstOrDefaultAsync();

    revokedToken
      .Should()
      .NotBeNull();

    revokedToken?.Revoked
      .Should()
      .BeTrue();
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndTokenIsRevoked_ItShouldReturn400StatusCodeWithProblemDetails()
  {
    var verificationToken = FakeDataFactory.VerificationToken.Generate() with
    {
      Revoked = true,
    };

    await Context.Tokens.InsertOneAsync(verificationToken);

    var verifyResponse = await _client.PostAsJsonAsync("/auth/verify-account", new
    {
      token = verificationToken.Token,
    });

    verifyResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.BadRequest);

    var verifyResponseBody = await verifyResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    verifyResponseBody
      .Should()
      .NotBeNull();

    verifyResponseBody?.Title
      .Should()
      .Be("Verification failed");

    verifyResponseBody?.Detail
      .Should()
      .Be("Unable to verify account. See errors for details.");

    verifyResponseBody?.Extensions
      .Should()
      .ContainKey("Errors");
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndTokenBelongsToNonExistentUser_ItShouldReturn404StatusCodeWithProblemDetails()
  {
    var verificationToken = FakeDataFactory.VerificationToken.Generate();
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();

    await Context.Tokens.InsertOneAsync(verificationToken);

    var verifyResponse = await _client.PostAsJsonAsync("/auth/verify-account", new
    {
      token = verificationToken.Token,
    });

    verifyResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.NotFound);

    var verifyResponseBody = await verifyResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    verifyResponseBody
      .Should()
      .NotBeNull();

    verifyResponseBody?.Title
      .Should()
      .Be("Verification failed");

    verifyResponseBody?.Detail
      .Should()
      .Be("Unable to verify account. See errors for details.");

    verifyResponseBody?.Extensions
      .Should()
      .ContainKey("Errors");

    var revokedToken = await Context.Tokens
      .Find(t => t.Id == verificationToken.Id)
      .FirstOrDefaultAsync();

    revokedToken
      .Should()
      .NotBeNull();
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndTokenBelongsToAlreadyVerifiedUser_ItShouldReturn409StatusCodeWithProblemDetails()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    existingUser.IsVerified = true;
    var verificationToken = FakeDataFactory.VerificationToken.Generate() with
    {
      UserId = existingUser.Id,
    };

    await Context.Tokens.InsertOneAsync(verificationToken);
    await Context.Users.InsertOneAsync(existingUser);

    var verifyResponse = await _client.PostAsJsonAsync("/auth/verify-account", new
    {
      token = verificationToken.Token,
    });

    verifyResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.Conflict);

    var verifyResponseBody = await verifyResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    verifyResponseBody
      .Should()
      .NotBeNull();

    verifyResponseBody?.Title
      .Should()
      .Be("Verification failed");

    verifyResponseBody?.Detail
      .Should()
      .Be("Unable to verify account. See errors for details.");

    verifyResponseBody?.Extensions
      .Should()
      .ContainKey("Errors");

    var revokedToken = await Context.Tokens
      .Find(t => t.Id == verificationToken.Id)
      .FirstOrDefaultAsync();

    revokedToken
      .Should()
      .NotBeNull();
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndTokenValid_ItShouldReturn204StatusCodeAndVerifyAccount()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var verificationToken = FakeDataFactory.VerificationToken.Generate() with
    {
      UserId = existingUser.Id,
    };

    await Context.Tokens.InsertOneAsync(verificationToken);
    await Context.Users.InsertOneAsync(existingUser);

    var verifyResponse = await _client.PostAsJsonAsync("/auth/verify-account", new
    {
      token = verificationToken.Token,
    });

    verifyResponse.StatusCode
      .Should()
      .Be(HttpStatusCode.NoContent);

    var verifyResponseBody = await verifyResponse.Content.ReadAsStringAsync();

    verifyResponseBody
      .Should()
      .BeEmpty();

    var verifiedUser = await Context.Users
      .Find(u => u.Id == existingUser.Id)
      .FirstOrDefaultAsync();

    verifiedUser
      .Should()
      .NotBeNull();

    verifiedUser?.IsVerified
      .Should()
      .BeTrue();

    var revokedToken = await Context.Tokens
      .Find(t => t.Id == verificationToken.Id)
      .FirstOrDefaultAsync();

    revokedToken
      .Should()
      .BeNull();
  }
}