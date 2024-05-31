
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

  /// <summary>
  /// Get a graph's API key
  /// </summary>
  /// <param name="request">The request as a <see cref="GetGraphKeyRequest"/> instance</param>
  /// <returns>An <see cref="Task"/> of <see cref="IResult"/></returns>
  public static async Task<IResult> GetGraphKey([AsParameters] GetGraphKeyRequest request)
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

    var getUserResult = await request.UserService.GetUserByIdAsync(userId);

    if (getUserResult.IsFailed && getUserResult.Errors.Exists(e => e is UserDoesNotExistError))
    {
      return Results.Problem(
        title: "Failed to get user",
        detail: "Unable to retrieve user. See errors for details.",
        statusCode: StatusCodes.Status404NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", getUserResult.Errors } }
      );
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

    var decryptedApiKey = await request.EncryptionService.DecryptForUserAsync(getGraphResult.Value.ApiKey, getUserResult.Value);

    var getGraphApiKeyResponse = new GetGraphKeyResponse(decryptedApiKey);

    return Results.Ok(getGraphApiKeyResponse);
  }

  /// <summary>
  /// Deletes a graph
  /// </summary>
  public static async Task<IResult> DeleteGraph([AsParameters] DeleteGraphRequest request)
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

    var title = "Failed to delete graph";
    var detail = "Unable to delete graph. See errors for details.";

    var getUserResult = await request.UserService.GetUserByIdAsync(userId);

    if (getUserResult.IsFailed && getUserResult.Errors.Exists(e => e is UserDoesNotExistError))
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status404NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", getUserResult.Errors } }
      );
    }

    var getGraphResult = await request.GraphService.GetGraphAsync(request.Id, userId);

    if (getGraphResult.IsFailed && getGraphResult.Errors.Exists(e => e is GraphNotFoundError))
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status404NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", getGraphResult.Errors } }
      );
    }

    var deleteGraphResult = await request.GraphService.DeleteGraphAsync(getGraphResult.Value.Id);

    if (deleteGraphResult.IsFailed)
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status500InternalServerError,
        extensions: new Dictionary<string, object?> { { "Errors", deleteGraphResult.Errors } }
      );
    }

    return Results.NoContent();
  }

  /// <summary>
  /// Updates a graph
  /// </summary>
  public static async Task<IResult> UpdateGraph([AsParameters] UpdateGraphRequest request)
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

    var validationResult = await request.Validator.ValidateAsync(request.Dto);

    if (validationResult.IsValid is false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var title = "Failed to update graph";
    var detail = "Unable to update graph. See errors for details.";

    var getUserResult = await request.UserService.GetUserByIdAsync(userId);

    if (getUserResult.IsFailed)
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status404NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", getUserResult.Errors } }
      );
    }

    var getGraphResult = await request.GraphService.GetGraphAsync(request.Id, userId);

    if (getGraphResult.IsFailed && getGraphResult.Errors.Exists(e => e is GraphNotFoundError))
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status404NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", getGraphResult.Errors } }
      );
    }

    var updatedGraph = request.Dto.ToGraph(getGraphResult.Value.ApiKey);

    var updateGraphResult = await request.GraphService.UpdateGraphAsync(updatedGraph);

    if (updateGraphResult.IsFailed && updateGraphResult.Errors.Exists(e => e is GraphAlreadyExistsError))
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status409Conflict,
        extensions: new Dictionary<string, object?> { { "Errors", updateGraphResult.Errors } }
      );
    }

    if (updateGraphResult.IsFailed)
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status500InternalServerError,
        extensions: new Dictionary<string, object?> { { "Errors", updateGraphResult.Errors } }
      );
    }

    var getUpdatedGraphResult = await request.GraphService.GetGraphAsync(request.Id, userId);

    if (getUpdatedGraphResult.IsFailed)
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status500InternalServerError,
        extensions: new Dictionary<string, object?> { { "Errors", getUpdatedGraphResult.Errors } }
      );
    }

    var graphDto = new GraphDto(getUpdatedGraphResult.Value);

    return Results.Ok(graphDto);
  }

  /// <summary>
  /// Updates a graph's key
  /// </summary>
  internal static async Task<IResult> UpdateGraphKey([AsParameters] UpdateGraphKeyRequest request)
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

    var validationResult = await request.Validator.ValidateAsync(request.Dto);

    if (validationResult.IsValid is false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var title = "Failed to update graph key";
    var detail = "Unable to update graph key. See errors for details.";

    var getUserResult = await request.UserService.GetUserByIdAsync(userId);

    if (getUserResult.IsFailed)
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status404NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", getUserResult.Errors } }
      );
    }

    var getGraphResult = await request.GraphService.GetGraphAsync(request.Id, userId);

    if (getGraphResult.IsFailed && getGraphResult.Errors.Exists(e => e is GraphNotFoundError))
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status404NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", getGraphResult.Errors } }
      );
    }

    var encryptedApiKey = await request.EncryptionService.EncryptForUserAsync(request.Dto.Key, getUserResult.Value);
    getGraphResult.Value.ApiKey = encryptedApiKey;

    var updateGraphResult = await request.GraphService.UpdateGraphAsync(getGraphResult.Value);

    if (updateGraphResult.IsFailed)
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status500InternalServerError,
        extensions: new Dictionary<string, object?> { { "Errors", updateGraphResult.Errors } }
      );
    }

    return Results.NoContent();
  }

  /// <summary>
  /// Refresh a graph
  /// </summary>
  internal static async Task<IResult> RefreshGraph([AsParameters] RefreshGraphRequest request)
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

    var title = "Failed to refresh graph";
    var detail = "Unable to refresh graph. See errors for details.";

    var getUserResult = await request.UserService.GetUserByIdAsync(userId);

    if (getUserResult.IsFailed)
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status404NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", getUserResult.Errors } }
      );
    }

    var getGraphResult = await request.GraphService.GetGraphAsync(request.Id, userId);

    if (getGraphResult.IsFailed && getGraphResult.Errors.Exists(e => e is GraphNotFoundError))
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status404NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", getGraphResult.Errors } }
      );
    }

    getGraphResult.Value.Status = GraphStatus.Building;

    var updateGraphResult = await request.GraphService.UpdateGraphAsync(getGraphResult.Value);

    if (updateGraphResult.IsFailed)
    {
      return Results.Problem(
        title: title,
        detail: detail,
        statusCode: StatusCodes.Status500InternalServerError,
        extensions: new Dictionary<string, object?> { { "Errors", updateGraphResult.Errors } }
      );
    }

    await request.GraphQueue.EnqueueAsync(new GraphQueueItem(getGraphResult.Value));

    return Results.NoContent();
  }
}