namespace Server.API.Tests.Integration;

public class UsersControllerTests(TestServerFactory serverFactory) : IntegrationTest(serverFactory)
{
  [Fact]
  public async Task GetUser_WhenCalledByUnauthenticatedUser_ItShouldReturnUnauthorized()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    user.IsVerified = true;

    await Context.Users.InsertOneAsync(user);

    var response = await _client.GetAsync($"/users/{user.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetUser_WhenCalledAndNotGivenInvalidUserId_ItShouldReturnBadRequest()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.GetAsync("/users/invalid-user-id");

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var responseBody = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    responseBody!.Errors["UserId"].Should().Contain("Invalid user id");
  }

  [Fact]
  public async Task GetUser_WhenCalledByDifferentUser_ItShouldReturnForbidden()
  {
    var (_, userRequesting) = FakeDataFactory.TestUser.Generate();
    var (_, userRequested) = FakeDataFactory.TestUser.Generate();

    var userRequestingJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, userRequesting.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userRequestingJwtToken);

    await Context.Users.InsertOneAsync(userRequested);

    var response = await _client.GetAsync($"/users/{userRequested.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task GetUser_WhenCalledAndUserDoesNotExist_ItShouldReturnNotFound()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.GetAsync($"/users/{ObjectId.GenerateNewId()}");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);

    var responseBody = await response.Content.ReadFromJsonAsync<ProblemDetails>();

    responseBody!.Title.Should().Be("Failed to get user");
    responseBody!.Detail.Should().Be("Unable to retrieve user. See errors for details.");
    responseBody!.Extensions.Should().ContainKey("Errors");
  }

  [Fact]
  public async Task GetUser_WhenCalledAndUserExists_ItShouldReturnUser()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    user.IsVerified = true;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);

    var response = await _client.GetAsync($"/users/{user.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var responseBody = await response.Content.ReadFromJsonAsync<UserResponse>();

    responseBody!.Id.Should().Be(user.Id);
    responseBody!.Username.Should().Be(user.Username);
  }
}