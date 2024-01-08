namespace Server.API.Middleware;

/// <summary>
/// Middleware for handling errors
/// </summary>
/// <remarks>
/// Creates a new <see cref="ErrorMiddleware"/> instance
/// </remarks>
/// <param name="next">The next middleware in the pipeline</param>
/// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance</param>
/// <returns>A <see cref="ErrorMiddleware"/> instance</returns>
class ErrorMiddleware(RequestDelegate next, ILogger<ErrorMiddleware> logger)
{
  private readonly RequestDelegate _next = next;
  private readonly ILogger<ErrorMiddleware> _logger = logger;

  /// <summary>
  /// Invokes the middleware
  /// </summary>
  /// <param name="context">The <see cref="HttpContext"/> instance</param>
  /// <returns>A <see cref="Task"/></returns>
  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred while processing the request");

      var problem = new ProblemDetails
      {
        Status = (int)HttpStatusCode.InternalServerError,
        Title = "An problem occurred while processing the request",
        Detail = ex.Message,
      };

      context.Response.StatusCode = problem.Status.Value;
      context.Response.ContentType = "application/problem+json";
      await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
  }
}