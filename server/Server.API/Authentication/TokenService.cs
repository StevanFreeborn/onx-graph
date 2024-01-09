

namespace Server.API.Authentication;

/// <summary>
/// A service for managing tokens.
/// </summary>
/// <inheritdoc cref="ITokenService"/>
class TokenService : ITokenService
{
  public string GenerateAccessToken(User existingUser)
  {
    throw new NotImplementedException();
  }

  public Task<Result<RefreshToken>> GenerateRefreshToken(string id)
  {
    throw new NotImplementedException();
  }
}