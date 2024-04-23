namespace Server.API.Graphs;

static class GraphsRoutes
{
  internal static RouteGroupBuilder MapVersionOneGraphsEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("graphs");

    group
      .MapPost("add", GraphsController.AddGraph)
      .RequireAuthorization()
      .Produces((int)HttpStatusCode.Created)
      .Produces((int)HttpStatusCode.Unauthorized)
      .ProducesValidationProblem()
      .Produces((int)HttpStatusCode.NotFound)
      .Produces((int)HttpStatusCode.InternalServerError)
      .WithName("AddGraph")
      .WithDescription("Adds a graph");

    return group;
  }
}