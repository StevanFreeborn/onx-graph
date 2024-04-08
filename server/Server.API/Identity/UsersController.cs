namespace Server.API.Identity;

static class UsersController
{
  public static async Task<IResult> GetUser([AsParameters] GetUserRequest req)
  {
    var userId = req.Context.GetUserId();

    if (userId is null)
    {
      return Results.Unauthorized();
    }

    if (ObjectId.TryParse(req.UserId, out var _) is false)
    {
      return Results.ValidationProblem(new Dictionary<string, string[]>()
      {
        { nameof(req.UserId), [ "Invalid user id"] }
      });
    }

    var userResult = await req.UserService.GetUserByIdAsync(req.UserId);

    if (userResult.IsFailed)
    {
      return Results.Problem(
        title: "Failed to get user",
        detail: "Unable to retrieve user. See errors for details.",
        statusCode: StatusCodes.Status404NotFound,
        extensions: new Dictionary<string, object?>()
        {
          { "Errors", userResult.Errors }
        }
      );
    }

    if (userResult.Value.Id != userId)
    {
      return Results.Forbid();
    }

    return Results.Ok(new UserResponse(userResult.Value));
  }
}