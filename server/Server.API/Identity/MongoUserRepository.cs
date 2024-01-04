namespace Server.API.Identity;

class MongoUserRepository(MongoDbContext context) : IUserRepository
{
  private readonly MongoDbContext _context = context;
}