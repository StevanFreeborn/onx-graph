namespace Server.API.Tests.Unit;

public class UserServiceTests
{
  private readonly Mock<ITokenService> _tokenServiceMock = new();
  private readonly Mock<IUserRepository> _userRepositoryMock = new();
  private readonly Mock<ILogger<UserService>> _loggerMock = new();
  private readonly Mock<IEncryptionService> _encryptionServiceMock = new();
  private readonly UserService _sut;

  public UserServiceTests()
  {
    _sut = new UserService(
      _tokenServiceMock.Object,
      _userRepositoryMock.Object,
      _loggerMock.Object,
      _encryptionServiceMock.Object
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
  public async Task RegisterUserAsync_WhenUserDoesNotExist_ItShouldCreateNewUserWithUsernameHashedPasswordAndEncryptionKeyThenReturnUserId()
  {
    var plainTextKey = "test123";
    var encryptedKey = "EncryptedKey";
    var unHashedPassword = "@Password1234";

    var newUser = new User
    {
      Email = "test@test.com",
      Password = unHashedPassword,
    };

    var createdUser = new User
    {
      Id = ObjectId.GenerateNewId().ToString(),
      Email = newUser.Email,
    };

    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(null as User);

    _userRepositoryMock
      .Setup(u => u.GetUserByUsernameAsync(It.IsAny<string>()))
      .ReturnsAsync(null as User);

    _encryptionServiceMock
      .Setup(e => e.GenerateKey())
      .Returns(plainTextKey);

    _encryptionServiceMock
      .Setup(e => e.EncryptAsync(plainTextKey))
      .ReturnsAsync(encryptedKey);

    _userRepositoryMock
      .Setup(u => u.CreateUserAsync(It.IsAny<User>()))
      .ReturnsAsync(createdUser);

    var result = await _sut.RegisterUserAsync(newUser);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
    result.Value.Should().Be(createdUser.Id);

    _userRepositoryMock
      .Verify(
        u => u.CreateUserAsync(
          It.Is<User>(
            u =>
              string.IsNullOrWhiteSpace(u.Username) == false &&
              u.EncryptionKey == encryptedKey &&
              u.Password != unHashedPassword
          )
        ),
        Times.Once
      );
  }

  [Fact]
  public async Task LoginUserAsync_WhenUserDoesNotExist_ItShouldReturnInvalidLoginError()
  {
    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(null as User);

    var result = await _sut.LoginUserAsync("test@test.com", "@Password1234");

    result.IsFailed
      .Should()
      .BeTrue();

    result.Errors
      .Should()
      .Contain(e => e is InvalidLoginError);
  }

  [Fact]
  public async Task LoginUserAsync_WhenPasswordIsInvalid_ItShouldReturnInvalidLoginError()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var password = existingUser.Password;
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(existingUser.Password);
    existingUser.Password = hashedPassword;

    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(existingUser);

    var result = await _sut.LoginUserAsync(existingUser.Email, "not the password");

    result.IsFailed
      .Should()
      .BeTrue();

    result.Errors
      .Should()
      .Contain(e => e is InvalidLoginError);
  }

  [Fact]
  public async Task LoginUserAsync_WhenRefreshTokenFailsToGenerate_ItShouldReturnLoginFailedError()
  {
    var (password, existingUser) = FakeDataFactory.TestUser.Generate();
    existingUser.IsVerified = true;

    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(existingUser);

    _tokenServiceMock
      .Setup(t => t.GenerateRefreshToken(It.IsAny<string>()))
      .ReturnsAsync(Result.Fail("Failed to generate refresh token."));

    var result = await _sut.LoginUserAsync(existingUser.Email, password);

    result.IsFailed
      .Should()
      .BeTrue();

    result.Errors
      .Should()
      .Contain(e => e is LoginFailedError);
  }

  [Fact]
  public async Task LoginUserAsync_WhenPasswordIsValidAndUserIsVerified_ItShouldReturnAccessTokenAndRefreshToken()
  {
    var (password, existingUser) = FakeDataFactory.TestUser.Generate();
    existingUser.IsVerified = true;

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

    result.IsSuccess
      .Should()
      .BeTrue();

    result.Value
      .AccessToken
      .Should()
      .NotBeEmpty();

    result.Value
      .RefreshToken
      .Should()
      .NotBeNull();

    result.Value
      .RefreshToken
      .Token
      .Should()
      .NotBeEmpty();

    result.Value
      .RefreshToken
      .ExpiresAt
      .Should()
      .BeAfter(DateTime.UtcNow);
  }

  [Fact]
  public async Task LoginUserAsync_WhenUserIsNotVerified_ItShouldReturnUserNotVerifiedError()
  {
    var (password, existingUser) = FakeDataFactory.TestUser.Generate();

    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(existingUser);

    var result = await _sut.LoginUserAsync(existingUser.Email, password);

    result.IsFailed
      .Should()
      .BeTrue();

    result.Errors
      .Should()
      .Contain(e => e is UserNotVerifiedError);
  }

  [Fact]
  public async Task GetUserByEmailAsync_WhenUserDoesNotExist_ItShouldReturnUserDoesNotExistError()
  {
    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(null as User);

    var result = await _sut.GetUserByEmailAsync("test@test.com");

    result.IsFailed
      .Should()
      .BeTrue();

    result.Errors
      .Should()
      .Contain(e => e is UserDoesNotExistError);
  }

  [Fact]
  public async Task GetUserByEmailAsync_WhenUserExists_ItShouldReturnUser()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    var result = await _sut.GetUserByEmailAsync(user.Email);

    result.IsSuccess
      .Should()
      .BeTrue();

    result.Value
      .Should()
      .Be(user);
  }

  [Fact]
  public async Task VerifyUserAsync_WhenUserDoesNotExist_ItShouldReturnUserDoesNotExistError()
  {
    _userRepositoryMock
      .Setup(u => u.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(null as User);

    var result = await _sut.VerifyUserAsync("123");

    result.IsFailed
      .Should()
      .BeTrue();

    result.Errors
      .Should()
      .Contain(e => e is UserDoesNotExistError);
  }

  [Fact]
  public async Task VerifyUserAsync_WhenUserIsAlreadyVerified_ItShouldReturnUserAlreadyVerifiedError()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    user.IsVerified = true;

    _userRepositoryMock
      .Setup(u => u.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    var result = await _sut.VerifyUserAsync(user.Id);

    result.IsFailed
      .Should()
      .BeTrue();

    result.Errors
      .Should()
      .Contain(e => e is UserAlreadyVerifiedError);
  }

  [Fact]
  public async Task VerifyUserAsync_WhenUserIsNotVerified_ItShouldVerifyUser()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _userRepositoryMock
      .Setup(u => u.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    var result = await _sut.VerifyUserAsync(user.Id);

    result.IsSuccess
      .Should()
      .BeTrue();

    _userRepositoryMock
      .Verify(
        u => u.UpdateUserAsync(
          It.Is<User>(u => u.IsVerified)
        ),
        Times.Once
      );
  }

  [Fact]
  public async Task GetUserByIdAsync_WhenUserDoesNotExist_ItShouldReturnUserDoesNotExistError()
  {
    _userRepositoryMock
      .Setup(u => u.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(null as User);

    var result = await _sut.GetUserByIdAsync("123");

    result.IsFailed
      .Should()
      .BeTrue();

    result.Errors
      .Should()
      .Contain(e => e is UserDoesNotExistError);
  }

  [Fact]
  public async Task GetUserByIdAsync_WhenUserExists_ItShouldReturnUser()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _userRepositoryMock
      .Setup(u => u.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    var result = await _sut.GetUserByIdAsync(user.Id);

    result.IsSuccess
      .Should()
      .BeTrue();

    result.Value
      .Should()
      .Be(user);
  }
}