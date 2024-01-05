namespace Server.API.Tests.Unit;

public class UserServiceTests
{
  private readonly Mock<IUserRepository> _userRepositoryMock = new();
  private readonly Mock<ILogger<UserService>> _loggerMock = new();

  [Fact]
  public async Task RegisterUserAsync_WhenUserAlreadyExists_ItShouldReturnUserAlreadyExistError()
  {
    _userRepositoryMock
      .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(Mock.Of<User>());

    var sut = new UserService(
      _userRepositoryMock.Object,
      _loggerMock.Object
    );

    var result = await sut.RegisterUserAsync(Mock.Of<User>());

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

    var sut = new UserService(
      _userRepositoryMock.Object,
      _loggerMock.Object
    );

    var result = await sut.RegisterUserAsync(newUser);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
    result.Value.Should().Be(createdUser.Id);
  }
}