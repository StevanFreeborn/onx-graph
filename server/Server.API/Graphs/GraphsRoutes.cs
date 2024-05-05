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

    group
      .MapGet(string.Empty, GraphsController.GetGraphs)
      .RequireAuthorization()
      .Produces<Page<Graph>>((int)HttpStatusCode.OK)
      .Produces((int)HttpStatusCode.Unauthorized)
      .Produces((int)HttpStatusCode.InternalServerError)
      .WithName("GetGraphs")
      .WithDescription("Gets a page of graphs");

    group
      .MapGet("{id}", GraphsController.GetGraph)
      .RequireAuthorization()
      .Produces<Graph>((int)HttpStatusCode.OK)
      .Produces((int)HttpStatusCode.Unauthorized)
      .Produces((int)HttpStatusCode.NotFound)
      .Produces((int)HttpStatusCode.InternalServerError)
      .WithName("GetGraph")
      .WithDescription("Gets a graph by id");

    group.MapHub<GraphsHub>("/hub");

    return group;
  }
}