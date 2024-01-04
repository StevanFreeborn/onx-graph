namespace Server.API.Authentication;

/// <summary>
/// Controller for handling authentication requests
/// </summary>
static class AuthController
{
  internal static Task<IResult> Register()
  {
    // in order to register a user we need:
    // - request with user data
    //   - email
    //   - password

    // need to validate the request
    // - email is valid
    // - password is valid

    // need to check if the user already exists
    // - if the user exists, return an error

    // need to generate a unique username
    // - email username + random number

    // need to hash the password
    // - bcrypt

    // need to save the user to the database

    throw new NotImplementedException();
  }

  internal static Task<IResult> Login()
  {
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