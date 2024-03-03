namespace Server.API.Authentication;

/// <summary>
/// Represents a base token
/// </summary>
record BaseToken
{
  /// <summary>
  /// The token's id
  /// </summary>
  public string Id { get; init; } = string.Empty;

  /// <summary>
  /// The user's id that the token belongs to
  /// </summary>
  public string UserId { get; init; } = string.Empty;

  /// <summary>
  /// The token
  /// </summary>
  public string Token { get; init; } = string.Empty;

  /// <summary>
  /// The time the token expires
  /// </summary>
  public DateTime ExpiresAt { get; init; } = DateTime.UtcNow;

  /// <summary>
  /// Whether or not the token has been revoked
  /// </summary>
  public bool Revoked { get; init; } = false;

  /// <summary>
  /// The token type. See <see cref="TokenType"/>
  /// </summary>
  public TokenType TokenType { get; init; }

  /// <summary>
  /// The time the token was created
  /// </summary>
  public DateTime CreatedAt { get; init; } = DateTime.UtcNow;


  /// <summary>
  /// The time the token was last updated
  /// </summary>
  public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}