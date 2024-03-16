namespace Server.API.Authentication;

/// <summary>
/// A service for managing tokens.
/// </summary>
/// <inheritdoc cref="ITokenService"/>
class TokenService(
  ITokenRepository tokenRepository,
  IUserRepository userRepository,
  IOptions<JwtOptions> jwtOptions,
  TimeProvider timeProvider,
  ILogger<TokenService> logger
) : ITokenService
{
  private readonly ITokenRepository _tokenRepository = tokenRepository;
  private readonly IUserRepository _userRepository = userRepository;
  private readonly JwtOptions _jwtOptions = jwtOptions.Value;
  private readonly TimeProvider _timeProvider = timeProvider;
  private readonly ILogger<TokenService> _logger = logger;

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

  public async Task<Result<VerificationToken>> GenerateVerificationToken(string userId)
  {
    var expiresAt = _timeProvider
      .GetUtcNow()
      .AddMinutes(15)
      .UtcDateTime;

    var token = new VerificationToken
    {
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
        new GenerateVerificationTokenError().CausedBy(ex)
      );
    }
  }

  public async Task<Result<(string AccessToken, RefreshToken RefreshToken)>> RefreshAccessTokenAsync(string userId, string refreshToken)
  {
    var token = await _tokenRepository.GetTokenAsync(refreshToken);
    var user = await _userRepository.GetUserById(userId);

    if (token is null)
    {
      return Result.Fail(
        new TokenDoesNotExist(refreshToken)
      );
    }

    if (user is null)
    {
      return Result.Fail(
        new UserDoesNotExistError(userId)
      );
    }

    if (token.UserId != userId || token.Revoked)
    {
      // this is could be a malicious attempt to get a new access token
      // we should revoke the refresh token and all other refresh tokens for the tokens user
      _logger.LogWarning(
        "Refresh token {RefreshToken} belongs to user {TokenUser} and has been revoked or does not belong to calling user: {UserId}",
        refreshToken,
        token.UserId,
        userId
      );
      await _tokenRepository.RevokeAllRefreshTokensForUserAsync(token.UserId);
      return Result.Fail(
        new InvalidTokenError(refreshToken)
      );
    }

    if (token.ExpiresAt < _timeProvider.GetUtcNow().UtcDateTime)
    {
      await RevokeRefreshTokenAsync(userId, refreshToken);
      return Result.Fail(
        new ExpiredTokenError(refreshToken)
      );
    }

    var accessToken = GenerateAccessToken(user);
    var newRefreshTokenResult = await GenerateRefreshToken(userId);

    if (newRefreshTokenResult.IsFailed)
    {
      return Result.Fail(
        newRefreshTokenResult.Errors
      );
    }

    await RevokeRefreshTokenAsync(userId, refreshToken);
    await RemoveAllInvalidRefreshTokensAsync(userId);

    return Result.Ok((accessToken, newRefreshTokenResult.Value));
  }

  public async Task RemoveAllInvalidRefreshTokensAsync(string userId) => await _tokenRepository.RemoveAllInvalidRefreshTokensAsync(userId);

  public async Task RevokeRefreshTokenAsync(string userId, string refreshToken)
  {
    var token = await _tokenRepository.GetTokenAsync(refreshToken);

    if (token is null)
    {
      _logger.LogWarning(
        "Refresh token {RefreshToken} does not exist",
        refreshToken
      );
      return;
    }

    if (token.UserId != userId)
    {
      _logger.LogWarning(
        "Refresh token {RefreshToken} does not belong to user {UserId}",
        refreshToken,
        userId
      );
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