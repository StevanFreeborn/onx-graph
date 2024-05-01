using MongoDB.Bson.Serialization.Serializers;

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
        cm.SetIgnoreExtraElements(true);
        cm.MapIdProperty(u => u.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
        cm.MapProperty(u => u.Username).SetElementName("username");
        cm.MapProperty(u => u.Email).SetElementName("email");
        cm.MapProperty(u => u.Password).SetElementName("password");
        cm.MapProperty(u => u.CreatedAt).SetElementName("createdAt");
        cm.MapProperty(u => u.UpdatedAt).SetElementName("updatedAt");
        cm.MapProperty(u => u.IsVerified).SetElementName("isVerified");
        cm.MapProperty(u => u.EncryptionKey).SetElementName("encryptionKey");
      }
    );

    BsonClassMap.TryRegisterClassMap<BaseToken>(
      cm =>
      {
        cm.AutoMap();
        cm.SetIgnoreExtraElements(true);
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

    BsonClassMap.TryRegisterClassMap<Graph>(
      cm =>
      {
        cm.AutoMap();
        cm.SetIgnoreExtraElements(true);
        cm.MapIdProperty(g => g.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
        cm.MapProperty(g => g.UserId).SetElementName("userId");
        cm.MapProperty(g => g.Name).SetElementName("name");
        cm.MapProperty(g => g.ApiKey).SetElementName("apiKey");
        cm.MapProperty(g => g.CreatedAt).SetElementName("createdAt");
        cm.MapProperty(g => g.UpdatedAt).SetElementName("updatedAt");
        cm.MapProperty(g => g.Status).SetElementName("status");
        cm.MapProperty(g => g.Nodes).SetElementName("nodes");
        cm.MapProperty(g => g.EdgesMap).SetElementName("edgesMap");
      }
    );

    BsonClassMap.TryRegisterClassMap<App>(
      cm =>
      {
        cm.AutoMap();
        cm.SetIgnoreExtraElements(true);
        cm.MapIdProperty(a => a.Id).SetElementName("id");
        cm.MapProperty(a => a.Name).SetElementName("name");
        cm.UnmapMember(a => a.Href);
      }
    );

    BsonClassMap.TryRegisterClassMap<Field>(
      cm =>
      {
        cm.AutoMap();
        cm.SetIgnoreExtraElements(true);
        cm.MapIdProperty(f => f.Id).SetElementName("id");
        cm.MapProperty(f => f.AppId).SetElementName("appId");
        cm.MapProperty(f => f.Name).SetElementName("name");
        cm.MapProperty(f => f.Type).SetElementName("type");
        cm.MapProperty(f => f.IsUnique).SetElementName("isUnique");
        cm.MapProperty(f => f.IsRequired).SetElementName("isRequired");
      }
    );

    BsonClassMap.TryRegisterClassMap<ReferenceField>(
      cm =>
      {
        cm.AutoMap();
        cm.SetIgnoreExtraElements(true);
        cm.MapProperty(f => f.Multiplicity).SetElementName("multiplicity");
        cm.MapProperty(f => f.ReferencedAppId).SetElementName("referencedAppId");
      }
    );
  }
}