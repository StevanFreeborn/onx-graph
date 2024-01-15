namespace Server.API.Persistence.Mongo;

/// <summary>
/// Options for interacting with MongoDB
/// </summary>
class MongoDbOptions
{
  /// <summary>
  /// Gets or sets the connection string
  /// </summary>
  public string ConnectionString { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the database name
  /// </summary>
  public string DatabaseName { get; set; } = string.Empty;
}