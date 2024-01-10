
namespace Server.API.Authentication;

class MongoTokenRepository(MongoDbContext context) : ITokenRepository
{
  private readonly MongoDbContext _context = context;

  public async Task<RefreshToken> CreateTokenAsync(RefreshToken token)
  {
    await _context.Tokens.InsertOneAsync(token);
    return token;
  }
}