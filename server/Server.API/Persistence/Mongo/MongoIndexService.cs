
namespace Server.API.Persistence.Mongo;

class MongoIndexService(MongoDbContext context, ILogger<MongoIndexService> logger) : IHostedService
{
  private readonly MongoDbContext _context = context;
  private readonly ILogger<MongoIndexService> _logger = logger;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Adding graph indexes");
    await AddGraphIndexes();
    _logger.LogInformation("Graph indexes added");
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

  private async Task AddGraphIndexes()
  {
    var graphUserIdIndex = new CreateIndexModel<Graph>(Builders<Graph>.IndexKeys.Ascending(x => x.UserId));
    await _context.Graphs.Indexes.CreateOneAsync(graphUserIdIndex);
  }
}