namespace Server.API.Graphs;

/// <summary>
/// Controller for handling graph requests
/// </summary>
static class GraphsController
{
  /// <summary>
  /// Adds a graph
  /// </summary>
  /// <param name="request">The request as an <see cref="AddGraphRequest"/> instance</param>
  /// <returns>An <see cref="Task"/> of <see cref="IResult"/></returns>
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

    var graph = new Graph(request.Dto, getUserResult.Value);
    var encryptedApiKey = await request.EncryptionService.EncryptForUserAsync(graph.ApiKey, getUserResult.Value);
    graph.ApiKey = encryptedApiKey;

    var addGraphResult = await request.GraphService.AddGraphAsync(graph);

    if (addGraphResult.IsFailed && addGraphResult.Errors.Any(e => e is GraphAlreadyExistsError))
    {
      return Results.Problem(
        title: problemTitle,
        statusCode: StatusCodes.Status409Conflict,
        detail: problemDetail,
        extensions: new Dictionary<string, object?> { { "Errors", addGraphResult.Errors } }
      );
    }

    return Results.Created(
      uri: $"/graphs/{addGraphResult.Value.Id}",
      value: new AddGraphResponse(addGraphResult.Value.Id)
    );
  }

  /// <summary>
  /// Gets graphs
  /// </summary>
  /// <param name="request">The request as a <see cref="GetGraphsRequest"/> instance</param>
  /// <returns>An <see cref="Task"/> of <see cref="IResult"/></returns>
  public static async Task<IResult> GetGraphs([AsParameters] GetGraphsRequest request)
  {
    var userId = request.HttpContext.GetUserId();

    if (userId is null)
    {
      return Results.Unauthorized();
    }

    var getGraphsResult = await request.GraphService.GetGraphsAsync(request.PageNumber, request.PageSize, userId);
    var getGraphsResponse = new GetGraphsResponse(getGraphsResult.Value);

    return Results.Ok(getGraphsResponse);
  }
}