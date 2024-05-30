namespace Server.API.Identity;

/// <summary>
/// Represents a user
/// </summary>
class User
{
  /// <summary>
  /// Gets or sets the user's id
  /// </summary>
  public string Id { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the user's email
  /// </summary>
  public string Email { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the user's password
  /// </summary>
  public string Password { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the user's username
  /// </summary>
  public string Username { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the user's created date
  /// </summary>
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// Gets or sets the user's updated date
  /// </summary>
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// Gets or sets whether the user has been verified.
  /// </summary>
  public bool IsVerified { get; set; } = false;

  /// <summary>
  /// Gets or sets the user's encryption key
  /// </summary>
  public string EncryptionKey { get; set; } = string.Empty;
}