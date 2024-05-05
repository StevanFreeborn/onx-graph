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

    await request.GraphQueue.EnqueueAsync(new GraphQueueItem(graph));

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

  /// <summary>
  /// Gets a graph
  /// </summary>
  /// <param name="request">The request as a <see cref="GetGraphRequest"/> instance</param>
  /// <returns>An <see cref="Task"/> of <see cref="IResult"/></returns>
  public static async Task<IResult> GetGraph([AsParameters] GetGraphRequest request)
  {
    var userId = request.HttpContext.GetUserId();

    if (userId is null)
    {
      return Results.Unauthorized();
    }

    if (ObjectId.TryParse(request.Id, out var _) is false)
    {
      return Results.ValidationProblem(new Dictionary<string, string[]>()
      {
        { nameof(request.Id), [ "Invalid graph id"] }
      });
    }

    var getGraphResult = await request.GraphService.GetGraphAsync(request.Id, userId);

    if (getGraphResult.IsFailed && getGraphResult.Errors.Exists(e => e is GraphNotFoundError))
    {
      return Results.Problem(
        title: "Failed to get graph",
        detail: "Unable to retrieve graph. See errors for details.",
        statusCode: StatusCodes.Status404NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", getGraphResult.Errors } }
      );
    }

    var getGraphResponse = new GraphDto(getGraphResult.Value);

    return Results.Ok(getGraphResponse);
  }
}