
namespace Server.API.Identity;

class MongoUserRepository(MongoDbContext context) : IUserRepository
{
  private readonly MongoDbContext _context = context;

  public Task<User> CreateUserAsync(User user)
  {
    throw new NotImplementedException();
  }

  public Task<User?> GetUserByEmailAsync(string email)
  {
    throw new NotImplementedException();
  }

  public Task<User?> GetUserByUsernameAsync(string username)
  {
    throw new NotImplementedException();
  }
}