namespace Server.API.Authentication;

/// <summary>
/// An error indicating an invalid login.
/// </summary>
class InvalidLoginError : Error
{
  internal InvalidLoginError() : base("Email/Password combination is not valid")
  {
  }
}

/// <summary>
/// An error indicating that an attempt to login failed.
/// </summary>
class LoginFailedError : Error
{
  internal LoginFailedError() : base("Login failed")
  {
  }
}

/// <summary>
/// An error indicating that a refresh token was not generated.
/// </summary>
class GenerateRefreshTokenError : Error
{
  internal GenerateRefreshTokenError() : base("Failed to generate refresh token")
  {
  }
}

class GenerateVerificationTokenError : Error
{
  internal GenerateVerificationTokenError() : base("Failed to generate verification token")
  {
  }
}

/// <summary>
/// An error indicating that a token was not found.
/// </summary>
class TokenDoesNotExist : Error
{
  internal TokenDoesNotExist(string identifier) : base($"Token does not exist with identifier: {identifier}")
  {
  }
}

/// <summary>
/// An error indicating a token has expired.
/// </summary>
class ExpiredTokenError : Error
{
  internal ExpiredTokenError(string token) : base($"Token has expired with: {token}")
  {
  }
}

/// <summary>
/// An error indicating a token is invalid.
/// </summary>
class InvalidTokenError : Error
{
  internal InvalidTokenError(string token) : base($"Token is invalid with: {token}")
  {
  }
}