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