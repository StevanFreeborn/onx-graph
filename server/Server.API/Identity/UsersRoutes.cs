namespace Server.API.Identity;

static class UsersRoutes
{
  internal static RouteGroupBuilder MapVersionOneUsersEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("users");

    group
      .MapGet("{userId}", UsersController.GetUser)
      .RequireAuthorization()
      .Produces<UserResponse>((int)HttpStatusCode.OK)
      .ProducesValidationProblem()
      .Produces<ProblemDetails>((int)HttpStatusCode.Unauthorized)
      .Produces<ProblemDetails>((int)HttpStatusCode.Forbidden)
      .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
      .WithName("GetUser")
      .WithDescription("Gets a user");

    return group;
  }
}