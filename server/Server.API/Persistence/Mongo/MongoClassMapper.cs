namespace Server.API.Persistence.Mongo;

/// <summary>
/// Mapper for mapping classes to MongoDB
/// </summary>
public static class MongoClassMapper
{
  /// <summary>
  /// Registers class mappings
  /// </summary>
  public static void RegisterClassMappings()
  {
    BsonClassMap.TryRegisterClassMap<User>(
      cm =>
      {
        cm.AutoMap();
        cm.MapIdProperty(u => u.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
        cm.MapProperty(u => u.Username).SetElementName("username");
        cm.MapProperty(u => u.Email).SetElementName("email");
        cm.MapProperty(u => u.Password).SetElementName("password");
        cm.MapProperty(u => u.CreatedAt).SetElementName("createdAt");
        cm.MapProperty(u => u.UpdatedAt).SetElementName("updatedAt");
      }
    );

    BsonClassMap.TryRegisterClassMap<BaseToken>(
      cm =>
      {
        cm.AutoMap();
        cm.MapIdProperty(rt => rt.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
        cm.MapProperty(rt => rt.UserId).SetElementName("userId");
        cm.MapProperty(rt => rt.Token).SetElementName("token");
        cm.MapProperty(rt => rt.ExpiresAt).SetElementName("expiresAt");
        cm.MapProperty(rt => rt.Revoked).SetElementName("revoked");
        cm.MapProperty(rt => rt.TokenType).SetElementName("tokenType");
        cm.MapProperty(rt => rt.CreatedAt).SetElementName("createdAt");
        cm.MapProperty(rt => rt.UpdatedAt).SetElementName("updatedAt");
      }
    );
  }
}