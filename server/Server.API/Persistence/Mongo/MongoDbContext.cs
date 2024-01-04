namespace Server.API.Persistence.Mongo;

/// <summary>
/// Context for interacting with MongoDB
/// </summary>
class MongoDbContext
{
  private readonly MongoDbOptions _options;

  /// <summary>
  /// Creates a new <see cref="MongoDbContext"/> instance
  /// </summary>
  /// <param name="options">A <see cref="IOptions{MongoDbOptions}"/> instance</param>
  /// <returns>A <see cref="MongoDbContext"/> instance</returns>
  public MongoDbContext(IOptions<MongoDbOptions> options)
  {
    MongoClassMapper.RegisterClassMappings();
    _options = options.Value;
    var client = new MongoClient(_options.ConnectionString);
    var database = client.GetDatabase(_options.DatabaseName);
  }
}