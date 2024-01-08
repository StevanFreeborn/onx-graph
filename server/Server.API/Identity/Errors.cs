namespace Server.API.Identity;

/// <summary>
/// An error indicating that a user already exists.
/// </summary>
class UserAlreadyExistError : Error
{
  internal UserAlreadyExistError(string email) : base($"User already exists with email: {email}")
  {
  }
}