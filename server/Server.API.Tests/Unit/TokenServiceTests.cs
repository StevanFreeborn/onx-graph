namespace Server.API.Tests.Unit;

public class TokenServiceTests
{
  private readonly Mock<ITokenRepository> _tokenRepositoryMock = new();
  private readonly Mock<IUserRepository> _userRepositoryMock = new();
  private readonly Mock<IOptions<JwtOptions>> _jwtOptionsMock = new();
  private readonly Mock<TimeProvider> _timeProviderMock = new();
  private readonly Mock<ILogger<TokenService>> _loggerMock = new();
  private readonly TokenService _sut;

  public TokenServiceTests()
  {
    var jwtOptions = FakeDataFactory.JwtOption.Generate();

    _jwtOptionsMock
      .Setup(j => j.Value)
      .Returns(jwtOptions);

    _sut = new TokenService(
      _tokenRepositoryMock.Object,
      _userRepositoryMock.Object,
      _jwtOptionsMock.Object,
      _timeProviderMock.Object,
      _loggerMock.Object
    );
  }

  [Fact]
  public void GenerateAccessToken_WhenCalled_ShouldReturnAccessToken()
  {
    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    var (_, user) = FakeDataFactory.TestUser.Generate();

    var result = _sut.GenerateAccessToken(user);

    result.Should().NotBeNullOrEmpty();

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.ReadJwtToken(result);

    token.Issuer
      .Should()
      .Be(_jwtOptionsMock.Object.Value.Issuer);

    token.Audiences.
      Should()
      .Contain(_jwtOptionsMock.Object.Value.Audience);

    token.ValidTo
      .Should()
      .BeCloseTo(
        token.ValidFrom.AddMinutes(_jwtOptionsMock.Object.Value.ExpiryInMinutes),
        TimeSpan.FromSeconds(5)
      );

    token.Claims
      .Should()
      .Contain(c => c.Type == JwtRegisteredClaimNames.Jti);

    token.Claims
      .Should()
      .Contain(c => c.Type == JwtRegisteredClaimNames.Sub);

    token.Claims
      .Should()
      .Contain(c => c.Type == JwtRegisteredClaimNames.NameId);

    token.Claims
      .Should()
      .Contain(c => c.Type == JwtRegisteredClaimNames.Email);

    var subClaim = token.Claims
      .First(c => c.Type == JwtRegisteredClaimNames.Sub);

    subClaim.Value
      .Should().Be(user.Id);

    var nameIdClaim = token.Claims
      .First(c => c.Type == JwtRegisteredClaimNames.NameId);

    nameIdClaim.Value
      .Should().Be(user.Username);

    var emailClaim = token.Claims
      .First(c => c.Type == JwtRegisteredClaimNames.Email);

    emailClaim.Value
      .Should()
      .Be(user.Email);
  }

  [Fact]
  public async Task GenerateRefreshToken_WhenCalled_ShouldReturnRefreshToken()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var now = DateTimeOffset.UtcNow;

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(now);

    var result = await _sut.GenerateRefreshToken(user.Id);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Should().BeOfType<RefreshToken>();
    result.Value.UserId.Should().Be(user.Id);
    result.Value.Token.Should().NotBeNullOrEmpty();
    result.Value.ExpiresAt.Should().Be(now.AddHours(12).UtcDateTime);
  }

  [Fact]
  public async Task GenerateRefreshToken_WhenCalledAndTokenRepositoryThrowsException_ShouldReturnError()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    _tokenRepositoryMock
      .Setup(t => t.CreateTokenAsync(It.IsAny<RefreshToken>()))
      .ThrowsAsync(new Exception());

    var result = await _sut.GenerateRefreshToken(user.Id);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<GenerateRefreshTokenError>();
  }

  [Fact]
  public async Task RemovalAllInvalidRefreshTokensAsync_WhenCalled_ItShouldCallTokenRepository()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    await _sut.RemoveAllInvalidRefreshTokensAsync(user.Id);

    _tokenRepositoryMock.Verify(
      t => t.RemoveAllInvalidRefreshTokensAsync(user.Id),
      Times.Once
    );
  }

  [Fact]
  public async Task RevokeRefreshTokenAsync_WhenCalledWithValidRefreshToken_ItShouldRevokeToken()
  {
    var refreshToken = FakeDataFactory.RefreshToken.Generate();

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(refreshToken);

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    await _sut.RevokeRefreshTokenAsync(refreshToken.UserId, refreshToken.Token);

    _tokenRepositoryMock.Verify(
      t => t.UpdateTokenAsync(
        It.Is<BaseToken>(
          t => t.Revoked == true && t.UpdatedAt >= refreshToken.UpdatedAt
        )
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task RevokeRefreshTokenAsync_WhenCalledWithNonExistentRefreshToken_ItShouldNotRevokeToken()
  {
    var refreshToken = FakeDataFactory.RefreshToken.Generate();

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(null as RefreshToken);

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    await _sut.RevokeRefreshTokenAsync(refreshToken.UserId, refreshToken.Token);

    _tokenRepositoryMock.Verify(
      t => t.UpdateTokenAsync(
        It.IsAny<BaseToken>()
      ),
      Times.Never
    );
  }

  [Fact]
  public async Task RevokeRefreshTokenAsync_WhenCalledWithNonMatchingUserId_ItShouldNotRevokeToken()
  {
    var refreshToken = FakeDataFactory.RefreshToken.Generate();

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(refreshToken);

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    await _sut.RevokeRefreshTokenAsync("some-other-user-id", refreshToken.Token);

    _tokenRepositoryMock.Verify(
      t => t.UpdateTokenAsync(
        It.IsAny<BaseToken>()
      ),
      Times.Never
    );
  }

  [Fact]
  public async Task RefreshAccessTokenAsync_WhenCalledAndRefreshTokenDoesNotExist_ItShouldReturnError()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _userRepositoryMock
      .Setup(u => u.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(null as RefreshToken);

    var result = await _sut.RefreshAccessTokenAsync(user.Id, "non-existent-token");

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<TokenDoesNotExistError>();
  }

  [Fact]
  public async Task RefreshAccessTokenAsync_WhenCalledAndUserDoesNotExist_ItShouldReturnError()
  {
    var refreshToken = FakeDataFactory.RefreshToken.Generate();

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(refreshToken);

    _userRepositoryMock
      .Setup(u => u.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(null as User);

    var result = await _sut.RefreshAccessTokenAsync("non-existent-user-id", refreshToken.Token);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<UserDoesNotExistError>();
  }

  [Fact]
  public async Task RefreshAccessTokenAsync_WhenCalledAndGeneratingNewRefreshTokenFails_ItShouldReturnError()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var refreshToken = FakeDataFactory.RefreshToken.Generate() with { UserId = user.Id };

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(refreshToken);

    _userRepositoryMock
      .Setup(u => u.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    _tokenRepositoryMock
      .Setup(t => t.CreateTokenAsync(It.IsAny<RefreshToken>()))
      .ThrowsAsync(new Exception());

    var result = await _sut.RefreshAccessTokenAsync(user.Id, refreshToken.Token);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<GenerateRefreshTokenError>();
  }

  [Fact]
  public async Task RefreshAccessTokenAsync_WhenCalledWithExpiredRefreshToken_ItShouldReturnError()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var refreshToken = FakeDataFactory.RefreshToken.Generate() with
    {
      UserId = user.Id,
      ExpiresAt = DateTime.UtcNow.AddDays(-1)
    };

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(refreshToken);

    _userRepositoryMock
      .Setup(u => u.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    var result = await _sut.RefreshAccessTokenAsync(user.Id, refreshToken.Token);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<ExpiredTokenError>();
  }

  [Fact]
  public async Task RefreshAccessTokenAsync_WhenCalledWithRefreshTokenThatDoesNotBelongToUser_ItShouldReturnErrorAndRevokeUsersRefreshTokens()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var refreshToken = FakeDataFactory.RefreshToken.Generate() with { UserId = user.Id };

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(refreshToken);

    _userRepositoryMock
      .Setup(u => u.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    var result = await _sut.RefreshAccessTokenAsync("some-other-user-id", refreshToken.Token);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<InvalidTokenError>();

    _tokenRepositoryMock.Verify(
      t => t.RevokeAllRefreshTokensForUserAsync(refreshToken.UserId),
      Times.Once
    );
  }

  [Fact]
  public async Task RefreshAccessTokenAsync_WhenCalledWithRevokedRefreshToken_ItShouldReturnErrorAndRevokeUsersRefreshTokens()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var refreshToken = FakeDataFactory.RefreshToken.Generate() with
    {
      UserId = user.Id,
      Revoked = true
    };

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(refreshToken);

    _userRepositoryMock
      .Setup(u => u.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    var result = await _sut.RefreshAccessTokenAsync(user.Id, refreshToken.Token);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<InvalidTokenError>();

    _tokenRepositoryMock.Verify(
      t => t.RevokeAllRefreshTokensForUserAsync(user.Id),
      Times.Once
    );
  }

  [Fact]
  public async Task RefreshAccessTokenAsync_WhenCalledWithValidUserAndRefreshTokenAndGeneratingNewRefreshTokenSucceeds_ItShouldReturnNewAccessTokenAndRefreshToken()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var refreshToken = FakeDataFactory.RefreshToken.Generate() with { UserId = user.Id };

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(refreshToken);

    _userRepositoryMock
      .Setup(u => u.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    var result = await _sut.RefreshAccessTokenAsync(user.Id, refreshToken.Token);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.AccessToken.Should().NotBeNullOrEmpty();
    result.Value.RefreshToken.Should().NotBeNull();
    result.Value.RefreshToken.Should().BeOfType<RefreshToken>();
    result.Value.RefreshToken.UserId.Should().Be(user.Id);
    result.Value.RefreshToken.Token.Should().NotBeNullOrEmpty();
    result.Value.RefreshToken.ExpiresAt.Should().BeCloseTo(
      DateTime.UtcNow.AddHours(12),
      TimeSpan.FromSeconds(5)
    );
  }

  [Fact]
  public async Task GenerateVerificationToken_WhenCalled_ItShouldReturnVerificationToken()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var now = DateTimeOffset.UtcNow;

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(now);

    var result = await _sut.GenerateVerificationToken(user.Id);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Should().BeOfType<VerificationToken>();
    result.Value.UserId.Should().Be(user.Id);
    result.Value.Token.Should().NotBeNullOrEmpty();
    result.Value.ExpiresAt.Should().Be(now.AddMinutes(15).UtcDateTime);
  }

  [Fact]
  public async Task GenerateVerificationToken_WhenCalledAndTokenRepositoryThrowsException_ShouldReturnError()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    _tokenRepositoryMock
      .Setup(t => t.CreateTokenAsync(It.IsAny<VerificationToken>()))
      .ThrowsAsync(new Exception());

    var result = await _sut.GenerateVerificationToken(user.Id);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<GenerateVerificationTokenError>();
  }

  [Fact]
  public async Task RevokeUserVerificationTokens_WhenCalled_ItShouldCallTokenRepository()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    await _sut.RevokeUserVerificationTokensAsync(user.Id);

    _tokenRepositoryMock.Verify(
      t => t.RevokeUserVerificationTokensAsync(user.Id),
      Times.Once
    );
  }

  [Fact]
  public async Task VerifyVerificationTokenAsync_WhenCalledWithNonExistentToken_ItShouldReturnError()
  {
    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(null as VerificationToken);

    var result = await _sut.VerifyVerificationTokenAsync("non-existent-token");

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<TokenDoesNotExistError>();
  }

  [Fact]
  public async Task VerifyVerificationTokenAsync_WhenCalledWithExpiredToken_ItShouldReturnError()
  {
    var token = FakeDataFactory.VerificationToken.Generate() with
    {
      UpdatedAt = DateTime.UtcNow.AddMinutes(-45),
      ExpiresAt = DateTime.UtcNow.AddMinutes(-30)
    };

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(token);

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    var result = await _sut.VerifyVerificationTokenAsync(token.Token);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<ExpiredTokenError>();
  }

  [Fact]
  public async Task VerifyVerificationTokenAsync_WhenCalledWithRevokedToken_ItShouldReturnError()
  {
    var token = FakeDataFactory.VerificationToken.Generate() with
    {
      Revoked = true
    };

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(token);

    var result = await _sut.VerifyVerificationTokenAsync(token.Token);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<InvalidTokenError>();
  }

  [Fact]
  public async Task VerifyVerificationTokenAsync_WhenCalledWithValidToken_ItShouldReturnError()
  {
    var token = FakeDataFactory.VerificationToken.Generate();

    _tokenRepositoryMock
      .Setup(t => t.GetTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(token);

    var result = await _sut.VerifyVerificationTokenAsync(token.Token);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(token);
  }

  [Fact]
  public async Task RevokeVerificationTokenAsync_WhenCalled_ItShouldCallTokenRepository()
  {
    var token = FakeDataFactory.VerificationToken.Generate();

    await _sut.RevokeVerificationTokenAsync(token.Token);

    _tokenRepositoryMock.Verify(
      t => t.RevokeVerificationTokenAsync(token.Token),
      Times.Once
    );
  }

  [Fact]
  public async Task RemovalAllInvalidVerificationTokensAsync_WhenCalled_ItShouldCallTokenRepository()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    await _sut.RemoveAllInvalidVerificationTokensAsync(user.Id);

    _tokenRepositoryMock.Verify(
      t => t.RemoveAllInvalidVerificationTokensAsync(user.Id),
      Times.Once
    );
  }
}