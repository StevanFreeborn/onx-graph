namespace Server.API.Configuration;

/// <summary>
/// Configures <see cref="CorsOptions"/>
/// </summary>
/// <remarks>
/// Creates a new <see cref="CorsOptionsSetup"/> instance
/// </remarks>
/// <param name="configuration">A <see cref="IConfiguration"/> instance</param>
/// <returns>A <see cref="CorsOptionsSetup"/> instance</returns>
class CorsOptionsSetup(IConfiguration configuration) : IConfigureOptions<CorsOptions>
{
  private const string SectionName = nameof(CorsOptions);
  private readonly IConfiguration _configuration = configuration;

  /// <summary>
  /// Configures <see cref="CorsOptions"/>
  /// </summary>
  /// <param name="options">A <see cref="CorsOptions"/> instance</param>
  /// <returns>A <see cref="CorsOptions"/> instance</returns>
  public void Configure(CorsOptions options)
  {
    _configuration
      .GetSection(SectionName)
      .Bind(options);
  }
}