
namespace Server.API.Identity;

interface IUserRepository
{
  Task<User> CreateUserAsync(User user);
  Task<User?> GetUserByEmailAsync(string email);
  Task<User?> GetUserByUsernameAsync(string username);
}