namespace Server.API.Authentication;

/// <summary>
/// Represents options used to configure JWT authentication
/// </summary>
class JwtOptions
{
  /// <summary>
  /// Gets or sets the secret used to sign the JWT
  /// </summary>
  public string Secret { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the issuer of the JWT
  /// </summary>
  public string Issuer { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the audience of the JWT
  /// </summary>
  public string Audience { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the expiry of the JWT in minutes
  /// </summary>
  public int ExpiryInMinutes { get; set; }
}