namespace Server.API.Tests.Unit;

public class TokenServiceTests
{
  private readonly Mock<ITokenRepository> _tokenRepositoryMock = new();
  private readonly Mock<IOptions<JwtOptions>> _jwtOptionsMock = new();
  private readonly Mock<TimeProvider> _timeProviderMock = new();

  [Fact]
  public void GenerateAccessToken_WhenCalled_ShouldReturnAccessToken()
  {
    var jwtOptions = FakeDataFactory.JwtOption.Generate();

    _jwtOptionsMock
      .Setup(j => j.Value)
      .Returns(jwtOptions);

    _timeProviderMock
      .Setup(t => t.GetUtcNow())
      .Returns(DateTime.UtcNow);

    var sut = new TokenService(
      _tokenRepositoryMock.Object,
      _jwtOptionsMock.Object,
      _timeProviderMock.Object
    );

    var user = FakeDataFactory.TestUser.Generate();

    var result = sut.GenerateAccessToken(user);

    result.Should().NotBeNullOrEmpty();

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.ReadJwtToken(result);

    token.Issuer
      .Should()
      .Be(jwtOptions.Issuer);

    token.Audiences.
      Should()
      .Contain(jwtOptions.Audience);

    token.ValidTo
      .Should()
      .BeCloseTo(
        token.ValidFrom.AddMinutes(jwtOptions.ExpiryInMinutes),
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
      .Should().Be(user.Email);
  }
}