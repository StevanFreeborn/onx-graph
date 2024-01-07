
namespace Server.API.Identity;

class MongoUserRepository(MongoDbContext context) : IUserRepository
{
  private readonly MongoDbContext _context = context;

  public async Task<User> CreateUserAsync(User user)
  {
    await _context.Users.InsertOneAsync(user);
    return user;
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