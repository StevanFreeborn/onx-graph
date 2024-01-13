using System.Runtime.Serialization;

using Bogus.DataSets;

namespace Server.API.Tests.Data;

/// <summary>
/// Factory for generating fake data
/// </summary>
static class FakeDataFactory
{
  /// <summary>
  /// Generates a new <see cref="User"/> instance
  /// </summary>
  internal static readonly UserGenerator TestUser = new();

  private static string GenerateJwtSecret()
  {
    var secret = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(secret);
    return Convert.ToBase64String(secret);
  }

  /// <summary>
  /// Generates a new <see cref="JwtOptions"/> instance
  /// </summary>
  internal static readonly Faker<JwtOptions> JwtOption = new Faker<JwtOptions>()
    .RuleFor(j => j.Audience, f => f.Internet.DomainName())
    .RuleFor(j => j.ExpiryInMinutes, f => f.Random.Int(1, 60))
    .RuleFor(j => j.Issuer, f => f.Internet.DomainName())
    .RuleFor(j => j.Secret, GenerateJwtSecret());

  internal static readonly Faker<RefreshToken> RefreshToken = new Faker<RefreshToken>()
    .CustomInstantiator(f => new RefreshToken())
    .RuleFor(t => t.Id, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(t => t.UserId, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(t => t.Token, f => f.Random.AlphaNumeric(32))
    .RuleFor(t => t.ExpiresAt, f => DateTime.UtcNow.AddHours(12))
    .RuleFor(t => t.Revoked, false)
    .RuleFor(t => t.TokenType, TokenType.Refresh);
}

/// <summary>
/// Generates a new user
/// </summary>
internal class UserGenerator
{
  private const string Password = "Password123!";

  private readonly Faker<User> _userFaker = new Faker<User>()
    .RuleFor(u => u.Id, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(u => u.Email, f => f.Person.Email)
    .RuleFor(u => u.Username, f => f.Person.UserName)
    .RuleFor(
      u => u.Password,
      f => BCrypt.Net.BCrypt.HashPassword(Password)
    );

  /// <summary>
  /// Generates a new user
  /// </summary>
  /// <returns>A tuple containing the user's password and the user with the password hashed as an <see cref="User"/> instance.</returns> 
  internal (string userPassword, User user) Generate() =>
    (Password, _userFaker.Generate());
}