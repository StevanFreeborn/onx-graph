namespace Server.API.Authentication;

/// <summary>
/// A service for managing tokens.
/// </summary>
/// <inheritdoc cref="ITokenService"/>
class TokenService(IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider) : ITokenService
{
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

  public Task<Result<RefreshToken>> GenerateRefreshToken(string id)
  {
    throw new NotImplementedException();
  }
}