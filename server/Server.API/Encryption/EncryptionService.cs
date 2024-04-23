
namespace Server.API.Encryption;

class EncryptionService(IOptions<EncryptionOptions> options) : IEncryptionService
{
  private readonly IOptions<EncryptionOptions> _options = options;

  private async Task<string> EncryptCore(string plainText, string key)
  {
    using var aes = Aes.Create();
    aes.Key = Encoding.UTF8.GetBytes(key);
    aes.IV = _options.Value.IV;

    var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

    using var memoryStream = new MemoryStream();
    using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
    using var streamWriter = new StreamWriter(cryptoStream);

    await streamWriter.WriteAsync(plainText);
    await streamWriter.FlushAsync();
    streamWriter.Close();

    return Convert.ToBase64String(memoryStream.ToArray());
  }

  private async Task<string> DecryptCore(string cipherText, string key)
  {
    using var aes = Aes.Create();
    aes.Key = Encoding.UTF8.GetBytes(key);
    aes.IV = _options.Value.IV;

    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

    using var memoryStream = new MemoryStream(Convert.FromBase64String(cipherText));
    using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
    using var streamReader = new StreamReader(cryptoStream);

    return await streamReader.ReadToEndAsync();
  }

  public async Task<string> EncryptAsync(string plainText) =>
    await EncryptCore(plainText, _options.Value.Key);

  public async Task<string> DecryptAsync(string cipherText) =>
    await DecryptCore(cipherText, _options.Value.Key);

  public async Task<string> EncryptForUserAsync(string plainText, User user)
  {
    var decryptedUserKey = await DecryptCore(user.EncryptionKey, _options.Value.Key);
    return await EncryptCore(plainText, decryptedUserKey);
  }

  public async Task<string> DecryptForUserAsync(string cipherText, User user)
  {
    var decryptedUserKey = await DecryptCore(user.EncryptionKey, _options.Value.Key);
    return await DecryptCore(cipherText, decryptedUserKey);
  }

  public string GenerateKey()
  {
    var key = new byte[16];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(key);

    return Convert.ToBase64String(key);
  }
}