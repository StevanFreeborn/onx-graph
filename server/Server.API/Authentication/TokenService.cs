namespace Server.API.Authentication;

/// <summary>
/// A service for managing tokens.
/// </summary>
/// <inheritdoc cref="ITokenService"/>
class TokenService(
  ITokenRepository tokenRepository,
  IOptions<JwtOptions> jwtOptions,
  TimeProvider timeProvider
) : ITokenService
{
  private readonly ITokenRepository _tokenRepository = tokenRepository;
  private readonly JwtOptions _jwtOptions = jwtOptions.Value;
  private readonly TimeProvider _timeProvider = timeProvider;

  public string GenerateAccessToken(User user)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(_jwtOptions.Secret);
    var issuedAt = _timeProvider.GetUtcNow();
    var expires = issuedAt.AddMinutes(_jwtOptions.ExpiryInMinutes);
    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new(JwtRegisteredClaimNames.Sub, user.Id),
      new(JwtRegisteredClaimNames.NameId, user.Username),
      new(JwtRegisteredClaimNames.Email, user.Email),
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Expires = expires.UtcDateTime,
      IssuedAt = issuedAt.UtcDateTime,
      Issuer = _jwtOptions.Issuer,
      Audience = _jwtOptions.Audience,
      SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256Signature
      )
    };

    var securityToken = tokenHandler.CreateToken(tokenDescriptor);
    var jwtToken = tokenHandler.WriteToken(securityToken);
    return jwtToken;
  }

  public async Task<Result<RefreshToken>> GenerateRefreshToken(string userId)
  {
    var expiresAt = _timeProvider
      .GetUtcNow()
      .AddHours(12)
      .UtcDateTime;

    var token = new RefreshToken
    {
      Id = Guid.NewGuid().ToString(),
      UserId = userId,
      Token = GenerateToken(),
      ExpiresAt = expiresAt
    };

    try
    {
      var createdToken = await _tokenRepository.CreateTokenAsync(token);
      return Result.Ok(token);
    }
    catch (Exception ex)
    {
      return Result.Fail(
        new GenerateRefreshTokenError().CausedBy(ex)
      );
    }
  }

  public async Task RemoveAllInvalidRefreshTokensAsync(string userId) => await _tokenRepository.RemoveAllInvalidRefreshTokensAsync(userId);

  public async Task RevokeRefreshTokenAsync(string userId, string refreshToken)
  {
    var token = await _tokenRepository.GetTokenAsync(refreshToken);

    if (token is null || token.UserId != userId)
    {
      return;
    }

    var updatedToken = token with
    {
      Revoked = true,
      UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime
    };

    await _tokenRepository.UpdateTokenAsync(updatedToken);
  }

  private string GenerateToken()
  {
    var randomBytes = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomBytes);
    var token = Convert.ToBase64String(randomBytes);
    return RemoveNonAlphaNumericCharacters(token);
  }

  private string RemoveNonAlphaNumericCharacters(string input)
  {
    var pattern = @"[^A-Za-z0-9]";
    var replacement = string.Empty;
    var output = Regex.Replace(input, pattern, replacement);
    return output;
  }
}