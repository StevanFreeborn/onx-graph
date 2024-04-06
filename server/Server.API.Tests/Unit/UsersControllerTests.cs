namespace Server.API.Tests.Unit;

public class UsersControllerTests
{
  private readonly Mock<HttpContext> _context = new();
  private readonly Mock<IUserService> _userService = new();

  private GetUserRequest CreateGetUserRequest(string userId)
  {
    return new GetUserRequest(userId, _context.Object, _userService.Object);
  }

  [Fact]
  public async Task GetUser_WhenCalledByUnauthenticatedUser_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    _context
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal());

    var request = CreateGetUserRequest("user-id");

    var result = await UsersController.GetUser(request);

    result.Should()
      .BeOfType<UnauthorizedHttpResult>();

    result.As<UnauthorizedHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status401Unauthorized);
  }

  [Fact]
  public async Task GetUser_WhenCalledByDifferentUser_ItShouldReturn403StatusCodeWithProblemDetails()
  {
    throw new NotImplementedException();
  }

  [Fact]
  public async Task GetUser_WhenCalledAndUserDoesNotExist_ItShouldReturn404StatusCodeWithProblemDetails()
  {
    throw new NotImplementedException();
  }

  [Fact]
  public async Task GetUser_WhenCalledAndUserExists_ItShouldReturn200StatusCodeWithUser()
  {
    throw new NotImplementedException();
  }
}