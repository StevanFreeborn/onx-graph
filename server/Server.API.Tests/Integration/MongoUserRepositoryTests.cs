namespace Server.API.Tests.Integration;

public class MongoUserRepositoryTests(TestDb testDb) : IClassFixture<TestDb>, IDisposable
{
  private readonly MongoDbContext _context = testDb.Context;
  private readonly MongoUserRepository _sut = new(testDb.Context);

  public void Dispose()
  {
    _context.Users.DeleteMany(u => true);
    GC.SuppressFinalize(this);
  }

  [Fact]
  public async Task CreateUserAsync_WhenCalled_ItShouldCreateUser()
  {
    var (_, newUser) = FakeDataFactory.TestUser.Generate();

    var result = await _sut.CreateUserAsync(newUser);

    result.Id
      .Should()
      .NotBeNullOrEmpty();

    var createdUser = await _context.Users
      .Find(u => u.Id == result.Id)
      .SingleOrDefaultAsync();

    createdUser
      .Should()
      .NotBeNull();
  }

  [Fact]
  public async Task GetUserByEmailAsync_WhenUserExists_ItShouldReturnUser()
  {
    var (_, testUser) = FakeDataFactory.TestUser.Generate();

    await _context.Users.InsertOneAsync(testUser);

    var result = await _sut.GetUserByEmailAsync(testUser.Email);

    result
      .Should()
      .NotBeNull();

    result?.Email
      .Should()
      .Be(testUser.Email);
  }

  [Fact]
  public async Task GetUserByEmailAsync_WhenUserDoesNotExist_ItShouldReturnNull()
  {
    var (_, testUser) = FakeDataFactory.TestUser.Generate();

    var result = await _sut.GetUserByEmailAsync(testUser.Email);

    result
      .Should()
      .BeNull();
  }

  [Fact]
  public async Task GetUserByUsernameAsync_WhenUserExists_ItShouldReturnUser()
  {
    var (_, testUser) = FakeDataFactory.TestUser.Generate();

    await _context.Users.InsertOneAsync(testUser);

    var result = await _sut.GetUserByUsernameAsync(testUser.Username);

    result
      .Should()
      .NotBeNull();

    result?.Username
      .Should()
      .Be(testUser.Username);
  }

  [Fact]
  public async Task GetUserByUsernameAsync_WhenUserDoesNotExist_ItShouldReturnNull()
  {
    var (_, testUser) = FakeDataFactory.TestUser.Generate();

    var result = await _sut.GetUserByUsernameAsync(testUser.Username);

    result
      .Should()
      .BeNull();
  }
}