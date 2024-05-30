namespace Server.API.Authentication;

/// <summary>
/// Configures <see cref="JwtOptions"/>
/// </summary>
/// <remarks>
/// Creates a new <see cref="JwtOptionsSetup"/> instance
/// </remarks>
/// <param name="configuration">A <see cref="IConfiguration"/> instance</param>
/// <returns>A <see cref="JwtOptionsSetup"/> instance</returns>
class JwtOptionsSetup(IConfiguration configuration) : IConfigureOptions<JwtOptions>
{
  private const string SectionName = nameof(JwtOptions);
  private readonly IConfiguration _configuration = configuration;

  /// <summary>
  /// Configures <see cref="JwtOptions"/>
  /// </summary>
  /// <param name="options">A <see cref="JwtOptions"/> instance</param>
  /// <returns>A <see cref="JwtOptions"/> instance</returns>
  public void Configure(JwtOptions options)
  {
    _configuration
      .GetSection(SectionName)
      .Bind(options);
  }
}