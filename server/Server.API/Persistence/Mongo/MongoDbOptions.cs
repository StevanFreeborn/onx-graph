namespace Server.API.Persistence.Mongo;

/// <summary>
/// Options for interacting with MongoDB
/// </summary>
class MongoDbOptions
{
  /// <summary>
  /// The connection string
  /// </summary>
  public string ConnectionString { get; set; } = string.Empty;

  /// <summary>
  /// The database name
  /// </summary>
  public string DatabaseName { get; set; } = string.Empty;
}