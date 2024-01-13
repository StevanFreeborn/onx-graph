namespace Server.API.Authentication;

/// <summary>
/// Auth routes
/// </summary>
static class AuthRoutes
{
  /// <summary>
  /// Maps auth endpoints
  /// </summary>
  internal static void MapAuthEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("auth");

    group
      .MapPost("register", AuthController.Register)
      .Produces<RegisterUserResponse>((int)HttpStatusCode.Created)
      .ProducesValidationProblem()
      .Produces<ProblemDetails>((int)HttpStatusCode.Conflict)
      .Produces<ProblemDetails>((int)HttpStatusCode.InternalServerError)
      .WithName("RegisterUser")
      .WithDescription("Register a new user");

    group
      .MapPost("login", AuthController.Login)
      .Produces<LoginUserResponse>((int)HttpStatusCode.OK)
      .ProducesValidationProblem()
      .Produces<ProblemDetails>((int)HttpStatusCode.Unauthorized)
      .Produces<ProblemDetails>((int)HttpStatusCode.InternalServerError)
      .WithName("LoginUser")
      .WithDescription("Login a user");

    group
      .MapPost("logout", AuthController.Logout)
      .RequireAuthorization()
      .Produces<ProblemDetails>((int)HttpStatusCode.Unauthorized)
      .Produces((int)HttpStatusCode.OK)
      .WithName("LogoutUser")
      .WithDescription("Logout a user");

    group
      .MapPost("refresh-token", AuthController.RefreshToken)
      .WithName("RefreshToken")
      .WithDescription("Refresh a user's token");
  }
}