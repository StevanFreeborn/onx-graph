namespace Server.API.Graphs;

static class GraphsController
{
  public static async Task<IResult> AddGraph([AsParameters] AddGraphRequest request)
  {
    var userId = request.HttpContext.GetUserId();

    if (userId is null)
    {
      return Results.Unauthorized();
    }

    var validationResult = await request.Validator.ValidateAsync(request.Dto);

    if (validationResult.IsValid is false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var problemTitle = "Add Graph Failed";
    var problemDetail = "Unable to add graph. See errors for details.";

    var getUserResult = await request.UserService.GetUserByIdAsync(userId);

    if (getUserResult.IsFailed)
    {
      return Results.Problem(
        title: problemTitle,
        statusCode: StatusCodes.Status404NotFound,
        detail: problemDetail,
        extensions: new Dictionary<string, object?> { { "Errors", getUserResult.Errors } }
      );
    }

    var graph = new Graph(request.Dto);
    var encryptedApiKey = await request.EncryptionService.EncryptForUser(graph.ApiKey, getUserResult.Value);
    graph.ApiKey = encryptedApiKey;

    var addGraphResult = await request.GraphService.AddGraph(graph);

    if (addGraphResult.IsFailed)
    {
      return Results.Problem(
        title: problemTitle,
        statusCode: StatusCodes.Status500InternalServerError,
        detail: problemDetail,
        extensions: new Dictionary<string, object?> { { "Errors", addGraphResult.Errors } }
      );
    }

    return Results.Created(
      uri: $"/graphs/{addGraphResult.Value.Id}",
      value: new AddGraphResponse(addGraphResult.Value.Id)
    );
  }
}