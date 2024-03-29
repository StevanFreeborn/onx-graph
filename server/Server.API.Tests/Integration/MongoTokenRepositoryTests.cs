namespace Server.API.Tests.Unit;

public class MongoTokenRepositoryTests : IClassFixture<TestDb>, IDisposable
{
  private readonly Mock<TimeProvider> _timeProviderMock = new();
  private readonly MongoDbContext _context;
  private readonly MongoTokenRepository _sut;

  public MongoTokenRepositoryTests(TestDb testDb)
  {
    _context = testDb.Context;
    _sut = new MongoTokenRepository(testDb.Context, _timeProviderMock.Object);
  }

  public void Dispose()
  {
    _context.Tokens.DeleteMany(t => true);
    GC.SuppressFinalize(this);
  }

  [Fact]
  public async Task CreateTokenAsync_WhenCalled_ItShouldCreateToken()
  {
    var newToken = FakeDataFactory.RefreshToken.Generate();

    var result = await _sut.CreateTokenAsync(newToken);

    result.Id
      .Should()
      .NotBeNullOrEmpty();

    var createdToken = await _context.Tokens
      .Find(t => t.Id == result.Id)
      .SingleOrDefaultAsync();

    createdToken
      .Should()
      .NotBeNull();
  }

  [Fact]
  public async Task GetTokenAsync_WhenTokenExists_ItShouldReturnToken()
  {
    var testToken = FakeDataFactory.RefreshToken.Generate();

    await _context.Tokens.InsertOneAsync(testToken);

    var result = await _sut.GetTokenAsync(testToken.Token);

    result
      .Should()
      .NotBeNull();

    result?.Token
      .Should()
      .Be(testToken.Token);
  }

  [Fact]
  public async Task GetTokenAsync_WhenTokenDoesNotExist_ItShouldReturnNull()
  {
    var testToken = FakeDataFactory.RefreshToken.Generate();

    var result = await _sut.GetTokenAsync(testToken.Token);

    result
      .Should()
      .BeNull();
  }

  [Fact]
  public async Task RemoveAllInvalidRefreshTokensAsync_WhenCalled_ItShouldRemoveTokens()
  {
    var userId = ObjectId.GenerateNewId().ToString();
    var tokens = FakeDataFactory.RefreshToken.Generate(3);

    var testTokens = tokens
      .Select(t => t with
      {
        UserId = userId
      })
      .ToList();

    var revokedToken = testTokens[0] with
    {
      Revoked = true
    };

    var expiredToken = testTokens[1] with
    {
      ExpiresAt = DateTime.UtcNow.AddDays(-1)
    };

    var unexpiredOrRevokedToken = testTokens[2] with
    {
      Revoked = false,
      ExpiresAt = DateTime.UtcNow.AddDays(1)
    };

    await _context.Tokens.InsertManyAsync(
      new List<RefreshToken>
      {
        revokedToken,
        expiredToken,
        unexpiredOrRevokedToken
      }
    );

    _timeProviderMock
      .Setup(x => x.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    await _sut.RemoveAllInvalidRefreshTokensAsync(userId);

    var result = await _context.Tokens
      .Find(t => t.UserId == userId)
      .ToListAsync();

    result
      .Should()
      .HaveCount(1);

    result[0].Id
      .Should()
      .Be(unexpiredOrRevokedToken.Id);
  }

  [Fact]
  public async Task UpdateTokenAsync_WhenCalled_ItShouldUpdateToken()
  {
    var testToken = FakeDataFactory.RefreshToken.Generate();

    await _context.Tokens.InsertOneAsync(testToken);

    var updatedToken = testToken with
    {
      Revoked = true
    };

    await _sut.UpdateTokenAsync(updatedToken);

    var result = await _context.Tokens
      .Find(t => t.Id == testToken.Id)
      .SingleOrDefaultAsync();

    result
      .Should()
      .NotBeNull();

    result?.Revoked
      .Should()
      .BeTrue();
  }

  [Fact]
  public async Task RevokeAllRefreshTokensAsync_WhenCalled_ItShouldRevokeTokens()
  {
    var userId = ObjectId.GenerateNewId().ToString();
    var tokens = FakeDataFactory.RefreshToken.Generate(3);

    var testTokens = tokens
      .Select(t => t with
      {
        UserId = userId
      })
      .ToList();

    var revokedToken = testTokens[0] with
    {
      Revoked = true
    };

    var unrevokedToken = testTokens[1] with
    {
      Revoked = false
    };

    var unrevokedToken2 = testTokens[2] with
    {
      Revoked = false
    };

    await _context.Tokens.InsertManyAsync(
      new List<RefreshToken>
      {
        revokedToken,
        unrevokedToken,
        unrevokedToken2
      }
    );

    await _sut.RevokeAllRefreshTokensForUserAsync(userId);

    var result = await _context.Tokens
      .Find(t => t.UserId == userId)
      .ToListAsync();

    result
      .Should()
      .HaveCount(3);

    result[0].Revoked
      .Should()
      .BeTrue();

    result[1].Revoked
      .Should()
      .BeTrue();

    result[2].Revoked
      .Should()
      .BeTrue();
  }
}