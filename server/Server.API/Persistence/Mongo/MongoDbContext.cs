namespace Server.API.Persistence.Mongo;

/// <summary>
/// Context for interacting with MongoDB
/// </summary>
class MongoDbContext
{
  private const string UsersCollectionName = "users";
  private const string TokensCollectionName = "tokens";
  private const string GraphsCollectionName = "graphs";
  private readonly MongoDbOptions _options;

  /// <summary>
  /// The users collection
  /// </summary>
  public IMongoCollection<User> Users { get; init; }

  /// <summary>
  /// The tokens collection
  /// </summary>
  public IMongoCollection<BaseToken> Tokens { get; init; }

  /// <summary>
  /// The graphs collection
  /// </summary>
  public IMongoCollection<Graph> Graphs { get; init; }

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
    Users = database.GetCollection<User>(UsersCollectionName);
    Tokens = database.GetCollection<BaseToken>(TokensCollectionName);
    Graphs = database.GetCollection<Graph>(GraphsCollectionName);
  }
}