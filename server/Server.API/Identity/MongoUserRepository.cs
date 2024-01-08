
namespace Server.API.Identity;

/// <summary>
/// A repository for managing users in MongoDB.
/// </summary>
/// <inheritdoc cref="IUserRepository"/>
class MongoUserRepository(MongoDbContext context) : IUserRepository
{
  private readonly MongoDbContext _context = context;

  public async Task<User> CreateUserAsync(User user)
  {
    await _context.Users.InsertOneAsync(user);
    return user;
  }

  public async Task<User?> GetUserByEmailAsync(string email)
  {
    return await _context.Users
      .Find(u => u.Email == email)
      .SingleOrDefaultAsync();
  }

  public async Task<User?> GetUserByUsernameAsync(string username)
  {
    return await _context.Users
      .Find(u => u.Username == username)
      .SingleOrDefaultAsync();
  }
}