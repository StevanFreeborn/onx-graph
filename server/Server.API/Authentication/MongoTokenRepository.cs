namespace Server.API.Authentication;

class MongoTokenRepository(MongoDbContext context) : ITokenRepository
{
  private readonly MongoDbContext _context = context;
}