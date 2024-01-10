namespace Server.API.Tests.Data;

public static class FakeDataFactory
{
  internal static readonly Faker<User> TestUser = new Faker<User>()
    .RuleFor(u => u.Id, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(u => u.Email, f => f.Person.Email)
    .RuleFor(u => u.Username, f => f.Person.UserName)
    .RuleFor(u => u.Password, "Password123!");

  private static string GenerateJwtSecret()
  {
    var secret = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(secret);
    return Convert.ToBase64String(secret);
  }

  internal static readonly Faker<JwtOptions> JwtOption = new Faker<JwtOptions>()
    .RuleFor(j => j.Audience, f => f.Internet.DomainName())
    .RuleFor(j => j.ExpiryInMinutes, f => f.Random.Int(1, 60))
    .RuleFor(j => j.Issuer, f => f.Internet.DomainName())
    .RuleFor(j => j.Secret, GenerateJwtSecret());
}