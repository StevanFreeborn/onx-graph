namespace Server.API.Encryption;

/// <summary>
/// A service for encrypting and decrypting data
/// </summary>
interface IEncryptionService
{
  /// <summary>
  /// Encrypts the specified plain text
  /// </summary>
  Task<string> Encrypt(string plainText);

  /// <summary>
  /// Encrypts the specified plain text using the given user's key
  /// </summary>
  Task<string> EncryptForUser(string plainText, User user);

  /// <summary>
  /// Decrypts the specified cipher text
  /// </summary>
  Task<string> Decrypt(string cipherText);

  /// <summary>
  /// Decrypts the specified cipher text using the given user's key
  /// </summary>
  Task<string> DecryptForUser(string cipherText, User user);

  /// <summary>
  /// Generate a random encryption key
  /// </summary>
  string GenerateKey();
}