namespace Server.API.Identity;

/// <summary>
/// A repository for managing users in MongoDB.
/// </summary>
/// <inheritdoc cref="IUserRepository"/>
class MongoUserRepository(MongoDbContext context, TimeProvider timeProvider) : IUserRepository
{
  private readonly MongoDbContext _context = context;
  private readonly TimeProvider _timeProvider = timeProvider;

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

  public async Task<User?> GetUserByIdAsync(string userId)
  {
    return await _context.Users
      .Find(u => u.Id == userId)
      .SingleOrDefaultAsync();
  }

  public async Task<User?> GetUserByUsernameAsync(string username)
  {
    return await _context.Users
      .Find(u => u.Username == username)
      .SingleOrDefaultAsync();
  }

  public Task UpdateUserAsync(User existingUser)
  {
    existingUser.UpdatedAt = _timeProvider.GetUtcNow().DateTime;
    var filter = Builders<User>.Filter.Eq(u => u.Id, existingUser.Id);
    return _context.Users.ReplaceOneAsync(filter, existingUser);
  }
}