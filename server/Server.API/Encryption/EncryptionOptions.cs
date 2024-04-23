namespace Server.API.Encryption;

/// <summary>
/// Represents the options used to configure encryption
/// </summary>
class EncryptionOptions
{
  /// <summary>
  /// The key used for encryption and decryption
  /// </summary>
  public string Key { get; set; } = string.Empty;

  /// <summary>
  /// The initialization vector used for encryption and decryption
  /// </summary>
  public byte[] IV { get; set; } = new byte[16];
}