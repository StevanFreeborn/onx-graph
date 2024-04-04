namespace Server.API.Tests.Integration;

public class UsersControllerTests(TestServerFactory serverFactory) : IntegrationTest(serverFactory), IDisposable
{
  public void Dispose()
  {
    Context.Users.DeleteMany(_ => true);
    GC.SuppressFinalize(this);
  }

  [Fact]
  public async Task GetUser_WhenCalledByUnauthenticatedUser_ReturnsUnauthorized()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    user.IsVerified = true;

    await Context.Users.InsertOneAsync(user);

    var response = await _client.GetAsync($"/api/users/{user.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetUser_WhenCalledByDifferentUser_ReturnsForbidden()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    user.IsVerified = true;

    await Context.Users.InsertOneAsync(user);

    var response = await _client.GetAsync($"/api/users/{user.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task GetUser_WhenCalledAndUserDoesNotExist_ReturnsNotFound()
  {
    var response = await _client.GetAsync($"/api/users/{ObjectId.GenerateNewId()}");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetUser_WhenCalledAndUserExists_ReturnsUser()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    user.IsVerified = true;

    await Context.Users.InsertOneAsync(user);

    var response = await _client.GetAsync($"/api/users/{user.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }
}