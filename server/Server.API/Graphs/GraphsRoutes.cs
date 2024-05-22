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

    group
      .MapGet("{id}/key", GraphsController.GetGraphKey)
      .RequireAuthorization()
      .Produces<GetGraphKeyResponse>((int)HttpStatusCode.OK)
      .Produces((int)HttpStatusCode.Unauthorized)
      .Produces((int)HttpStatusCode.NotFound)
      .Produces((int)HttpStatusCode.InternalServerError)
      .WithName("GetGraphKey")
      .WithDescription("Gets a graph's key by graph id");

    group
      .MapDelete("{id}", GraphsController.DeleteGraph)
      .RequireAuthorization()
      .Produces((int)HttpStatusCode.NoContent)
      .Produces((int)HttpStatusCode.Unauthorized)
      .Produces((int)HttpStatusCode.NotFound)
      .Produces((int)HttpStatusCode.InternalServerError)
      .WithName("DeleteGraph")
      .WithDescription("Deletes a graph by id");

    group
      .MapPut("{id}", GraphsController.UpdateGraph)
      .RequireAuthorization()
      .Produces((int)HttpStatusCode.NoContent)
      .Produces((int)HttpStatusCode.Unauthorized)
      .Produces((int)HttpStatusCode.NotFound)
      .Produces((int)HttpStatusCode.InternalServerError)
      .WithName("UpdateGraph")
      .WithDescription("Updates a graph by id");

    group
      .MapPatch("{id}/key", GraphsController.UpdateGraphKey)
      .RequireAuthorization()
      .Produces((int)HttpStatusCode.NoContent)
      .Produces((int)HttpStatusCode.Unauthorized)
      .Produces((int)HttpStatusCode.NotFound)
      .Produces((int)HttpStatusCode.InternalServerError)
      .WithName("UpdateGraphKey")
      .WithDescription("Updates a graph's key by graph id");

    return group;
  }
}