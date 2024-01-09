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

/// <summary>
/// An indicating that a user does not exist.
/// </summary>
class UserDoesNotExistError : Error
{
  internal UserDoesNotExistError(string email) : base($"User does not exist with email: {email}")
  {
  }
}