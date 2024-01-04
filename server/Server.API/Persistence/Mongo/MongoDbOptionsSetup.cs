namespace Server.API.Persistence.Mongo;

/// <summary>
/// Configures <see cref="MongoDbOptions"/>
/// </summary>
/// <remarks>
/// Creates a new <see cref="MongoDbOptionsSetup"/> instance
/// </remarks>
/// <param name="configuration">A <see cref="IConfiguration"/> instance</param>
/// <returns>A <see cref="MongoDbOptionsSetup"/> instance</returns>
class MongoDbOptionsSetup(IConfiguration configuration) : IConfigureOptions<MongoDbOptions>
{
  private const string SectionName = nameof(MongoDbOptions);
  private readonly IConfiguration _configuration = configuration;

  /// <summary>
  /// Configures <see cref="MongoDbOptions"/>
  /// </summary>
  /// <param name="options">A <see cref="MongoDbOptions"/> instance</param>
  /// <returns>A <see cref="MongoDbOptions"/> instance</returns>
  public void Configure(MongoDbOptions options)
  {
    _configuration
      .GetSection(SectionName)
      .Bind(options);
  }
}