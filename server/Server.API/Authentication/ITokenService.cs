
namespace Server.API.Authentication;

/// <summary>
/// A service for managing tokens.
/// </summary>
interface ITokenService
{
  /// <summary>
  /// Generates an access token for the given user.
  /// </summary>
  /// <param name="existingUser">The user to generate the token for.</param>
  /// <returns>The generated access token as a <see cref="string"/>.</returns>
  string GenerateAccessToken(User existingUser);

  /// <summary>
  /// Generates a refresh token for the given user.
  /// </summary>
  /// <param name="userId">The id of the user to generate the token for.</param>
  /// <returns>The generated refresh token as a <see cref="Result{T}"/>.</returns>
  Task<Result<RefreshToken>> GenerateRefreshToken(string userId);
  Task<Result<(string AccessToken, RefreshToken RefreshToken)>> RefreshAccessTokenAsync(string userId, string refreshToken);
  Task RemoveAllInvalidRefreshTokensAsync(string userId);

  Task RevokeRefreshTokenAsync(string userId, string refreshToken);
}