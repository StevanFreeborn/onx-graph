
namespace Server.API.Authentication;

/// <summary>
/// A repository for managing tokens.
/// </summary>
interface ITokenRepository
{
  /// <summary>
  /// Creates a refresh token.
  /// </summary>
  /// <param name="token">The token to create.</param>
  /// <returns>The created token as a <see cref="RefreshToken"/>.</returns>
  Task<RefreshToken> CreateTokenAsync(RefreshToken token);
  Task<BaseToken?> GetTokenAsync(string refreshToken);
  Task RemoveAllInvalidRefreshTokensAsync(string userId);
  Task UpdateTokenAsync(BaseToken updatedToken);
}