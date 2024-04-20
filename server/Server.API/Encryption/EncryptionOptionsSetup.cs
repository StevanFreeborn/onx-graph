namespace Server.API.Encryption;

/// <summary>
/// Configures <see cref="EncryptionOptions"/>
/// </summary>
/// <remarks>
/// Creates a new <see cref="EncryptionOptionsSetup"/> instance
/// </remarks>
/// <param name="configuration">A <see cref="IConfiguration"/> instance</param>
/// <returns>A <see cref="EncryptionOptionsSetup"/> instance</returns>
class EncryptionOptionsSetup(IConfiguration configuration) : IConfigureOptions<EncryptionOptions>
{
  private const string SectionName = nameof(EncryptionOptions);
  private readonly IConfiguration _configuration = configuration;

  /// <summary>
  /// Configures <see cref="EncryptionOptions"/>
  /// </summary>
  /// <param name="options">A <see cref="EncryptionOptions"/> instance</param>
  /// <returns>A <see cref="EncryptionOptions"/> instance</returns>
  public void Configure(EncryptionOptions options)
  {
    _configuration
      .GetSection(SectionName)
      .Bind(options);
  }
}