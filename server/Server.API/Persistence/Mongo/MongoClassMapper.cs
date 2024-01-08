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
  }
}