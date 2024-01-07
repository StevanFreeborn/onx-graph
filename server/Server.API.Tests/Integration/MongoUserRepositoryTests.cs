
using Microsoft.Extensions.Options;

using MongoDB.Driver;

namespace Server.API.Tests.Integration;

public class MongoUserRepositoryTests : IAsyncLifetime
{
  private readonly MongoDbContainer _container = new MongoDbBuilder().Build();

  public async Task InitializeAsync() => await _container.StartAsync();

  public async Task DisposeAsync() => await _container.DisposeAsync().AsTask();

  private MongoDbContext GetContext() => new(
    Options.Create(new MongoDbOptions
    {
      ConnectionString = _container.GetConnectionString(),
      DatabaseName = "test",
    })
  );

  [Fact]
  public async Task CreateUserAsync_WhenCalled_ItShouldCreateUser()
  {
    var newUser = new User
    {
      Email = "test@test.com",
      Password = "HashedTestPassword",
      Username = "TestUser",
    };

    var context = GetContext();

    var sut = new MongoUserRepository(context);

    var result = await sut.CreateUserAsync(newUser);

    result.Id.Should().NotBeNullOrEmpty();

    var createdUser = await context.Users
      .Find(u => u.Id == result.Id)
      .SingleOrDefaultAsync();

    createdUser.Should().NotBeNull();
  }
}