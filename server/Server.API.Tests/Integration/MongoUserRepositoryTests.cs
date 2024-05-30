namespace Server.API.Tests.Integration;

public class MongoUserRepositoryTests : IClassFixture<TestDb>
{
  private readonly Mock<TimeProvider> _timeProvider = new();
  private readonly MongoDbContext _context;
  private readonly MongoUserRepository _sut;

  public MongoUserRepositoryTests(TestDb testDb)
  {
    _timeProvider
      .Setup(x => x.GetUtcNow())
      .Returns(DateTime.UtcNow);

    _context = testDb.Context;
    _sut = new(testDb.Context, _timeProvider.Object);
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

  [Fact]
  public async Task UpdateUserAsync_WhenCalled_ItShouldUpdateUser()
  {
    var (_, testUser) = FakeDataFactory.TestUser.Generate();

    await _context.Users.InsertOneAsync(testUser);

    testUser.Username = "new-username";
    testUser.Email = "test@test.com";
    testUser.IsVerified = true;

    await _sut.UpdateUserAsync(testUser);

    var updatedUser = await _context.Users
      .Find(u => u.Id == testUser.Id)
      .SingleOrDefaultAsync();

    updatedUser
      .Should()
      .NotBeNull();

    updatedUser.Username
      .Should()
      .Be(testUser.Username);

    updatedUser.Email
      .Should()
      .Be(testUser.Email);

    updatedUser.IsVerified
      .Should()
      .BeTrue();
  }
}