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