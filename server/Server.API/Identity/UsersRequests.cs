namespace Server.API.Identity;

record GetUserRequest(
  [FromRoute] string UserId,
  HttpContext Context,
  [FromServices] IUserService UserService
);