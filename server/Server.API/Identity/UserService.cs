namespace Server.API.Identity;

class UserService(IUserRepository userRepository) : IUserService
{
  private readonly IUserRepository _userRepository = userRepository;
}