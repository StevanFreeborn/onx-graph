
namespace Server.API.Graphs;

/// <summary>
/// A repository for managing graphs in MongoDB.
/// </summary>
/// <inheritdoc cref="IGraphRepository"/>
class MongoGraphRepository(MongoDbContext context) : IGraphRepository
{
  private readonly MongoDbContext _context = context;

  public async Task<Graph> CreateGraphAsync(Graph graph)
  {
    await _context.Graphs.InsertOneAsync(graph);
    return graph;
  }

  public async Task<Graph?> GetGraphByNameAsync(string name, string userId)
  {
    return await _context.Graphs
      .Find(g => g.Name == name && g.UserId == userId)
      .FirstOrDefaultAsync();
  }
}