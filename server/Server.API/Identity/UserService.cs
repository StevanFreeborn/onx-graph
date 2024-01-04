using FluentResults;

namespace Server.API.Identity;

class UserService(IUserRepository userRepository) : IUserService
{
  private readonly IUserRepository _userRepository = userRepository;

  public Task<Result<string>> RegisterUserAsync(User user)
  {
    // TODO: implement this method
    // need to check if the user already exists
    // - if the user exists, return an error

    // need to generate a unique username
    // - email username + random number

    // need to hash the password
    // - bcrypt

    // need to save the user to the database
    throw new NotImplementedException();
  }
}