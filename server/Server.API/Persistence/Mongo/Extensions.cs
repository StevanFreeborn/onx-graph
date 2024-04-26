namespace Server.API.Persistence.Mongo;

static class IServicesCollectionExtensions
{
  public static async Task AddIndexes(this IServiceCollection services)
  {
    var context = services.BuildServiceProvider().GetRequiredService<MongoDbContext>();

    var graphUserIdIndex = new CreateIndexModel<Graph>(Builders<Graph>.IndexKeys.Ascending(x => x.Id));

    await context.Graphs.Indexes.CreateOneAsync(graphUserIdIndex);
  }
}
