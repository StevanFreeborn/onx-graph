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

    return Results.Ok();
  }
}