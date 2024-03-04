namespace Server.API.Identity;

/// <summary>
/// An error indicating that a user already exists.
/// </summary>
class UserAlreadyExistError : Error
{
  internal UserAlreadyExistError(string identifier) : base($"User already exists with identifier: {identifier}")
  {
  }
}

/// <summary>
/// An error indicating that a user does not exist.
/// </summary>
class UserDoesNotExistError : Error
{
  internal UserDoesNotExistError(string identifier) : base($"User does not exist with identifier: {identifier}")
  {
  }
}