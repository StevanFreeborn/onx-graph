namespace Server.API.Migrations;

[ExcludeFromCodeCoverage]
static class WebApplicationExtensions
{
  internal static async Task RunMigrations(this WebApplication app)
  {
    await app.EnsureUsersHaveEncryptionKeys();
  }

  private static async Task EnsureUsersHaveEncryptionKeys(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();
    var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

    try
    {
      logger.LogInformation("Ensuring all users have encryption keys.");

      var usersWithoutEncryptionKey = await context.Users
        .Find(u => u.EncryptionKey == null || u.EncryptionKey == string.Empty)
        .ToListAsync();

      if (usersWithoutEncryptionKey.Count is 0)
      {
        logger.LogInformation("All users have encryption keys.");
        return;
      }

      logger.LogInformation("Found {UserCount} users without encryption keys.", usersWithoutEncryptionKey.Count);

      foreach (var user in usersWithoutEncryptionKey)
      {
        var encryptionKey = encryptionService.GenerateKey();
        var encryptedKey = await encryptionService.EncryptAsync(encryptionKey);
        user.EncryptionKey = encryptedKey;

        await context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

        logger.LogInformation("Generated and encrypted encryption key for user {UserId}.", user.Id);
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred while ensuring all users have encryption keys.");
    }
  }
}