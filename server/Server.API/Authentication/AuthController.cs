namespace Server.API.Authentication;

/// <summary>
/// Controller for handling authentication requests
/// </summary>
static class AuthController
{
  /// <summary>
  /// Registers a new user.
  /// </summary>
  /// <param name="req">The request as an <see cref="RegisterRequest"/> instance.</param>
  /// <returns>A <see cref="Task"/> of <see cref="IResult"/>.</returns>
  internal static async Task<IResult> Register([AsParameters] RegisterRequest req)
  {
    var validationResult = await req.Validator.ValidateAsync(req.Dto);

    if (validationResult.IsValid == false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var registrationResult = await req.UserService.RegisterUserAsync(req.Dto.ToUser());

    if (registrationResult.IsFailed)
    {
      return Results.Problem(
        title: "Registration failed",
        detail: "Unable to register user. See errors for details.",
        statusCode: (int)HttpStatusCode.Conflict,
        extensions: new Dictionary<string, object?> { { "Errors", registrationResult.Errors } }
      );
    }

    return Results.Created(
      uri: $"/users/{registrationResult.Value}",
      value: new RegisterUserResponse(registrationResult.Value)
    );
  }

  internal static Task<IResult> Login()
  {
    // TODO: Implement

    // validate request. should have email and password
    // verify user exists
    // verify password is correct
    // generate jwt
    // generate refresh token
    // save refresh token
    // set cookie with refresh token
    // return jwt

    throw new NotImplementedException();
  }

  internal static Task<IResult> Logout()
  {
    throw new NotImplementedException();
  }

  internal static Task<IResult> RefreshToken()
  {
    throw new NotImplementedException();
  }
}