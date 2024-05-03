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

  /// <summary>
  /// Generates a new <see cref="RefreshToken"/> instance
  /// </summary>
  internal static readonly Faker<RefreshToken> RefreshToken = new Faker<RefreshToken>()
    .CustomInstantiator(f => new RefreshToken())
    .RuleFor(t => t.Id, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(t => t.UserId, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(t => t.Token, f => f.Random.AlphaNumeric(32))
    .RuleFor(t => t.ExpiresAt, f => DateTime.UtcNow.AddHours(12))
    .RuleFor(t => t.Revoked, false)
    .RuleFor(t => t.TokenType, TokenType.Refresh);

  /// <summary>
  /// Generates a new <see cref="SmtpOptions"/> instance
  /// </summary>
  internal static readonly Faker<SmtpOptions> SmtpOptions = new Faker<SmtpOptions>()
    .RuleFor(t => t.SmtpAddress, f => f.Internet.Ip())
    .RuleFor(t => t.SmtpPort, f => f.Random.Int(1, 65535))
    .RuleFor(t => t.SenderEmail, f => f.Internet.Email())
    .RuleFor(t => t.SenderPassword, f => string.Empty);

  /// <summary>
  /// Generates a new <see cref="EmailMessage"/> instance
  /// </summary>
  internal static readonly Faker<EmailMessage> EmailMessage = new Faker<EmailMessage>()
    .RuleFor(t => t.Subject, f => f.Lorem.Sentence())
    .RuleFor(t => t.HtmlContent, f => f.Lorem.Paragraphs(3))
    .RuleFor(t => t.To, f => f.Person.Email);

  /// <summary>
  /// Generates a new <see cref="VerificationToken"/> instance
  /// </summary>
  internal static readonly Faker<VerificationToken> VerificationToken = new Faker<VerificationToken>()
    .CustomInstantiator(f => new VerificationToken())
    .RuleFor(t => t.Id, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(t => t.UserId, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(t => t.Token, f => f.Random.AlphaNumeric(32))
    .RuleFor(t => t.ExpiresAt, f => DateTime.UtcNow.AddMinutes(15))
    .RuleFor(t => t.Revoked, false)
    .RuleFor(t => t.TokenType, TokenType.Verification);

  /// <summary>
  /// Generates a new <see cref="Graph"/> instance
  /// </summary>
  internal static readonly Faker<Graph> Graph = new Faker<Graph>()
    .CustomInstantiator(f => new Graph())
    .RuleFor(g => g.Id, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(g => g.UserId, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(g => g.Name, f => f.Lorem.Word())
    .RuleFor(g => g.ApiKey, f => f.Random.AlphaNumeric(32))
    .RuleFor(g => g.CreatedAt, f => DateTime.UtcNow)
    .RuleFor(g => g.UpdatedAt, f => DateTime.UtcNow);

  /// <summary>
  /// Generates a new <see cref="GraphQueueItem"/> instance
  /// </summary>
  internal static readonly Faker<GraphQueueItem> GraphQueueItem = new Faker<GraphQueueItem>()
    .CustomInstantiator(f => new GraphQueueItem())
    .RuleFor(i => i.Id, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(i => i.GraphId, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(i => i.UserId, f => ObjectId.GenerateNewId().ToString());
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