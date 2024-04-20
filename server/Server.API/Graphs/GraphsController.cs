namespace Server.API.Graphs;

static class GraphsController
{
  public static Task<IResult> AddGraph(AddGraphRequest request)
  {
    var userId = request.HttpContext.GetUserId();

    if (userId is null)
    {
      return Task.FromResult(Results.Unauthorized());
    }

    return Task.FromResult(Results.Created());
  }
}