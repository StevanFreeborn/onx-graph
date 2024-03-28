namespace Server.API.Authentication;

/// <summary>
/// A repository for managing tokens.
/// </summary>
interface ITokenRepository
{
  /// <summary>
  /// Creates a token.
  /// </summary>
  /// <param name="token">The token to create.</param>
  /// <returns>The created token as a <see cref="BaseToken"/>.</returns>
  Task<BaseToken> CreateTokenAsync(BaseToken token);

  /// <summary>
  /// Gets a token by its token string.
  /// </summary>
  /// <param name="token">The token string.</param>
  /// <param name="tokenType">The type of token.</param>
  /// <returns>The token as a <see cref="BaseToken"/> instance.</returns>
  Task<BaseToken?> GetTokenAsync(string token, TokenType tokenType);

  /// <summary>
  /// Removes all the invalid refresh tokens for a user.
  /// </summary>
  /// <param name="userId">The user id.</param>
  /// <returns>A <see cref="Task"/>.</returns>
  Task RemoveAllInvalidRefreshTokensAsync(string userId);

  /// <summary>
  /// Revokes all the refresh tokens for a user.
  /// </summary>
  /// <param name="userId">The user id.</param>
  /// <returns>A <see cref="Task"/>.</returns>
  Task RevokeAllRefreshTokensForUserAsync(string userId);

  /// <summary>
  /// Updates a token by replacing it with an updated token.
  /// </summary>
  /// <param name="updatedToken">The updated token.</param>
  /// <returns>A <see cref="Task"/>.</returns>
  Task UpdateTokenAsync(BaseToken updatedToken);

  /// <summary>
  /// Revoke all verification tokens for given user.
  /// </summary>
  /// <param name="userId">The user id.</param>
  /// <returns>A <see cref="Task"/>.</returns>
  Task RevokeUserVerificationTokensAsync(string userId);

  /// <summary>
  /// Revoke a verification token.
  /// </summary>
  /// <param name="token">The token to revoke.</param>
  /// <returns>A <see cref="Task"/>.</returns>
  Task RevokeVerificationTokenAsync(string token);
}