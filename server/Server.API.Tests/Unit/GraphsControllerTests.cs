namespace Server.API.Tests.Unit;

public class GraphsControllerTests
{
  private readonly Mock<HttpContext> _context = new();

  private AddGraphRequest CreateAddGraphRequest() => new(_context.Object);

  [Fact]
  public async Task AddGraph_WhenCalledByUnauthenticatedUser_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    _context
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal());

    var request = CreateAddGraphRequest();

    var result = await GraphsController.AddGraph(request);

    result.Should()
      .BeOfType<UnauthorizedHttpResult>();

    result.As<UnauthorizedHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status401Unauthorized);
  }
}