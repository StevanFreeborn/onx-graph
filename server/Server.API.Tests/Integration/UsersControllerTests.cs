namespace Server.API.Tests.Integration;

public class UsersControllerTests(TestServerFactory serverFactory) : IntegrationTest(serverFactory), IDisposable
{
  public void Dispose()
  {
    Context.Users.DeleteMany(_ => true);
    GC.SuppressFinalize(this);
  }

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
    var response = await _client.GetAsync("/users/invalid-user-id");

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GetUser_WhenCalledByDifferentUser_ItShouldReturnForbidden()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    user.IsVerified = true;

    await Context.Users.InsertOneAsync(user);

    var response = await _client.GetAsync($"/users/{user.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task GetUser_WhenCalledAndUserDoesNotExist_ItShouldReturnNotFound()
  {
    var response = await _client.GetAsync($"/users/{ObjectId.GenerateNewId()}");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetUser_WhenCalledAndUserExists_ItShouldReturnUser()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    user.IsVerified = true;

    await Context.Users.InsertOneAsync(user);

    var response = await _client.GetAsync($"/users/{user.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }
}