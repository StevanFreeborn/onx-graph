namespace Server.API.Tests.Unit;

public class UserServiceTests
{
  private readonly Mock<IUserRepository> _userRepositoryMock = new();
  private readonly Mock<ILogger<UserService>> _loggerMock = new();

  [Fact]
  public async Task RegisterUserAsync_WhenUserAlreadyExists_ItShouldReturnUserAlreadyExistError()
  {
    throw new NotImplementedException();
  }

  [Fact]
  public async Task RegisterUserAsync_WhenUserDoesNotExist_ItShouldCreateNewUserWithUsernameAndHashedPasswordReturnUserId()
  {
    throw new NotImplementedException();
  }
}