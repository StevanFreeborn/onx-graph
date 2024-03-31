namespace Server.API.Tests.Integration;

public class AccountMonitorTests(TestDb testDb) : IClassFixture<TestDb>, IDisposable
{
  private readonly Mock<ILogger<AccountMonitor>> _loggerMock = new();
  private readonly Mock<TimeProvider> _timeProviderMock = new();
  private readonly FakeTimeProvider _fakeTimeProvider = new();
  private readonly MongoDbContext _context = testDb.Context;

  public void Dispose()
  {
    _context.Users.DeleteMany(u => true);
    _context.Tokens.DeleteMany(t => true);
    GC.SuppressFinalize(this);
  }

  [Fact]
  public async Task StartAsync_WhenCalled_ItShouldCreateTimer()
  {
    var sut = new AccountMonitor(
      _loggerMock.Object,
      _timeProviderMock.Object,
      _context
    );

    await sut.StartAsync(CancellationToken.None);

    _timeProviderMock.Verify(
      x => x.CreateTimer(
        It.IsAny<TimerCallback>(),
        null,
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(15)
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task StartAsync_WhenCalledAnd15MinutesHavePast_ItShouldDeleteUnverifiedAccounts()
  {
    _fakeTimeProvider.SetUtcNow(DateTime.UtcNow);

    var (_, user) = FakeDataFactory.TestUser.Generate();
    user.CreatedAt = DateTime.UtcNow.AddHours(-49);

    var token = FakeDataFactory.RefreshToken.Generate() with { UserId = user.Id };

    await _context.Users.InsertOneAsync(user);
    await _context.Tokens.InsertOneAsync(token);

    var sut = new AccountMonitor(
      _loggerMock.Object,
      _fakeTimeProvider,
      _context
    );

    await sut.StartAsync(CancellationToken.None);

    _fakeTimeProvider.Advance(TimeSpan.FromMinutes(16));

    var existingUser = await _context.Users.Find(u => u.Id == user.Id).FirstOrDefaultAsync();
    var existingToken = await _context.Tokens.Find(t => t.UserId == user.Id).FirstOrDefaultAsync();

    existingUser.Should().BeNull();
    existingToken.Should().BeNull();
  }

  [Fact]
  public async Task StopAsync_WhenCalledBefore15MinutesHavePassed_ItShouldNotDeleteUnverifiedAccounts()
  {
    _fakeTimeProvider.SetUtcNow(DateTime.UtcNow);

    var (_, user) = FakeDataFactory.TestUser.Generate();
    user.CreatedAt = DateTime.UtcNow.AddHours(-49);

    var token = FakeDataFactory.RefreshToken.Generate() with { UserId = user.Id };

    await _context.Users.InsertOneAsync(user);
    await _context.Tokens.InsertOneAsync(token);

    var sut = new AccountMonitor(
      _loggerMock.Object,
      _fakeTimeProvider,
      _context
    );

    await sut.StartAsync(CancellationToken.None);

    _fakeTimeProvider.Advance(TimeSpan.FromMinutes(14));

    await sut.StopAsync(CancellationToken.None);

    _fakeTimeProvider.Advance(TimeSpan.FromMinutes(2));

    var existingUser = await _context.Users.Find(u => u.Id == user.Id).FirstOrDefaultAsync();
    var existingToken = await _context.Tokens.Find(t => t.UserId == user.Id).FirstOrDefaultAsync();

    existingUser.Should().NotBeNull();
    existingToken.Should().NotBeNull();
  }
}