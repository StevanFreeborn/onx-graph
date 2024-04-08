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
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _context
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, "different-user-id")
      ])));

    _userService
      .Setup(s => s.GetUserByIdAsync(user.Id))
      .ReturnsAsync(Result.Ok(user));

    var request = CreateGetUserRequest(user.Id);

    var result = await UsersController.GetUser(request);

    result.Should()
      .BeOfType<ForbidHttpResult>();
  }

  [Fact]
  public async Task GetUser_WhenCalledAndUserDoesNotExist_ItShouldReturn404StatusCodeWithProblemDetails()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var request = CreateGetUserRequest(user.Id);

    _context
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, user.Id)
      ])));

    _userService
      .Setup(s => s.GetUserByIdAsync(user.Id))
      .ReturnsAsync(Result.Fail<User>(new UserDoesNotExistError(user.Id)));

    var result = await UsersController.GetUser(request);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status404NotFound);

    result.As<ProblemHttpResult>()
      .ProblemDetails
      .Extensions
      .Should()
      .ContainKey("Errors");
  }

  [Fact]
  public async Task GetUser_WhenCalledAndUserExists_ItShouldReturn200StatusCodeWithUser()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var request = CreateGetUserRequest(user.Id);

    _context
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, user.Id)
      ])));

    _userService
      .Setup(s => s.GetUserByIdAsync(user.Id))
      .ReturnsAsync(Result.Ok(user));

    var result = await UsersController.GetUser(request);

    result.Should()
      .BeOfType<Ok<UserResponse>>();

    var okResult = result.As<Ok<UserResponse>>();

    okResult.StatusCode
      .Should()
      .Be(StatusCodes.Status200OK);

    okResult.Value
      .Should()
      .BeEquivalentTo(new UserResponse(user));
  }
}