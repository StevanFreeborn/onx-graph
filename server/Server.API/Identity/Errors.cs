namespace Server.API.Identity;

class UserAlreadyExistError : Error
{
  internal UserAlreadyExistError(string email) : base($"User already exists with email: {email}")
  {
  }
}