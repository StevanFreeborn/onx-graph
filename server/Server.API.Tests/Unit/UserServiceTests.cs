using Server.API.Tests.Data;

using Xunit.Abstractions;

namespace Server.API.Tests.Unit;

public class UserServiceTests
{
  private readonly Mock<ITokenService> _tokenServiceMock = new();
  private readonly Mock<IUserRepository> _userRepositoryMock = new();
  private readonly Mock<ILogger<UserService>> _loggerMock = new();
  private readonly UserService _sut;

  public UserServiceTests()
  {
    _sut = new UserService(
      _tokenServiceMock.Object,
      _userRepositoryMock.Object,
      _loggerMock.Object
    );
  }

  [Fact]
  public async Task RegisterUserAsync_WhenUserAlreadyExists_ItShouldReturnUserAlreadyExistError()
  {
    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(Mock.Of<User>());

    var result = await _sut.RegisterUserAsync(Mock.Of<User>());

    result.IsFailed.Should().BeTrue();
    result.Errors.Should()
      .Contain(e => e is UserAlreadyExistError);
  }

  [Fact]
  public async Task RegisterUserAsync_WhenUserDoesNotExist_ItShouldCreateNewUserWithUsernameAndHashedPasswordReturnUserId()
  {
    var newUser = new User
    {
      Email = "test@test.com",
      Password = "@Password1234",
    };

    var createdUser = new User
    {
      Id = ObjectId.GenerateNewId().ToString(),
      Email = newUser.Email,
      Username = "test123",
      Password = "HashedPassword",
    };

    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(null as User);

    _userRepositoryMock
      .Setup(u => u.GetUserByUsernameAsync(It.IsAny<string>()))
      .ReturnsAsync(null as User);

    _userRepositoryMock
      .Setup(u => u.CreateUserAsync(It.IsAny<User>()))
      .ReturnsAsync(createdUser);

    var result = await _sut.RegisterUserAsync(newUser);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
    result.Value.Should().Be(createdUser.Id);
  }

  [Fact]
  public async Task LoginUserAsync_WhenUserDoesNotExist_ItShouldReturnInvalidLoginError()
  {
    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(null as User);

    var result = await _sut.LoginUserAsync("test@test.com", "@Password1234");

    result.IsFailed.Should().BeTrue();
    result.Errors
      .Should()
      .Contain(e => e is InvalidLoginError);
  }

  [Fact]
  public async Task LoginUserAsync_WhenPasswordIsInvalid_ItShouldReturnInvalidLoginError()
  {
    var existingUser = FakeDataFactory.TestUser.Generate();
    var password = existingUser.Password;
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(existingUser.Password);
    existingUser.Password = hashedPassword;

    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(existingUser);

    var result = await _sut.LoginUserAsync(existingUser.Email, "not the password");

    result.IsFailed.Should().BeTrue();
    result.Errors
      .Should()
      .Contain(e => e is InvalidLoginError);
  }

  [Fact]
  public async Task LoginUserAsync_WhenRefreshTokenFailsToGenerate_ItShouldReturnLoginFailedError()
  {
    var existingUser = FakeDataFactory.TestUser.Generate();
    var password = existingUser.Password;
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(existingUser.Password);
    existingUser.Password = hashedPassword;

    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(existingUser);

    _tokenServiceMock
      .Setup(t => t.GenerateRefreshToken(It.IsAny<string>()))
      .ReturnsAsync(Result.Fail("Failed to generate refresh token."));

    var result = await _sut.LoginUserAsync(existingUser.Email, password);

    result.IsFailed.Should().BeTrue();
    result.Errors
      .Should()
      .Contain(e => e is LoginFailedError);
  }

  [Fact]
  public async Task LoginUserAsync_WhenPasswordIsValid_ItShouldReturnAccessTokenAndRefreshToken()
  {
    var existingUser = FakeDataFactory.TestUser.Generate();
    var password = existingUser.Password;
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(existingUser.Password);
    existingUser.Password = hashedPassword;

    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(existingUser);

    _tokenServiceMock
      .Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
      .Returns("AccessToken");

    _tokenServiceMock
      .Setup(t => t.GenerateRefreshToken(It.IsAny<string>()))
      .ReturnsAsync(Result.Ok(new RefreshToken
      {
        Token = "RefreshToken",
        ExpiresAt = DateTime.UtcNow.AddDays(7),
      }));

    var result = await _sut.LoginUserAsync(existingUser.Email, password);

    result.IsSuccess.Should().BeTrue();
    result.Value.AccessToken.Should().NotBeEmpty();
    result.Value.RefreshToken.Should().NotBeNull();
    result.Value.RefreshToken.Token.Should().NotBeEmpty();
    result.Value.RefreshToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
  }
}