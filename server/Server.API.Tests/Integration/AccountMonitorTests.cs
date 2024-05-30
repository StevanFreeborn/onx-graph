namespace Server.API.Tests.Integration;

public class AccountMonitorTests(TestDb testDb) : IClassFixture<TestDb>
{
  private readonly Mock<ILogger<AccountMonitor>> _loggerMock = new();
  private readonly Mock<TimeProvider> _timeProviderMock = new();
  private readonly FakeTimeProvider _fakeTimeProvider = new();
  private readonly MongoDbContext _context = testDb.Context;

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

    var (_, unverifiedOldUser) = FakeDataFactory.TestUser.Generate();
    unverifiedOldUser.CreatedAt = DateTime.UtcNow.AddHours(-49);

    var (_, verifiedOldUser) = FakeDataFactory.TestUser.Generate();
    verifiedOldUser.CreatedAt = DateTime.UtcNow.AddHours(-49);
    verifiedOldUser.IsVerified = true;

    var token = FakeDataFactory.RefreshToken.Generate() with { UserId = unverifiedOldUser.Id };

    await _context.Users.InsertOneAsync(unverifiedOldUser);
    await _context.Users.InsertOneAsync(verifiedOldUser);
    await _context.Tokens.InsertOneAsync(token);

    var sut = new AccountMonitor(
      _loggerMock.Object,
      _fakeTimeProvider,
      _context
    );

    await sut.StartAsync(CancellationToken.None);

    _fakeTimeProvider.Advance(TimeSpan.FromMinutes(16));

    var isDeleted = false;
    var retries = 10;

    do
    {
      var existingOldUnverifiedUser = await _context.Users.Find(u => u.Id == unverifiedOldUser.Id).FirstOrDefaultAsync();
      var existingToken = await _context.Tokens.Find(t => t.UserId == unverifiedOldUser.Id).FirstOrDefaultAsync();
      isDeleted = existingOldUnverifiedUser is null && existingToken is null;

      var delay = 1000;
      await Task.Delay(delay);
      retries--;
    } while (isDeleted is false && retries > 0);

    var existingOldVerifiedUser = await _context.Users.Find(u => u.Id == verifiedOldUser.Id).FirstOrDefaultAsync();

    existingOldVerifiedUser.Should().NotBeNull();
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