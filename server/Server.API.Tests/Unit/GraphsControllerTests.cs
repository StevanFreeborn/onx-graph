namespace Server.API.Tests.Unit;

public class GraphsControllerTests
{
  private readonly Mock<HttpContext> _context = new();

  private AddGraphRequest CreateAddGraphRequest() => new(_context.Object);

  public async Task AddGraph_WhenCalledByUnauthenticatedUser_ItShouldReturn401StatusCodeWithProblemDetails()
  {
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