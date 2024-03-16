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

  /// <summary>
  /// Generates a verification token for the given user.
  /// </summary>
  /// <param name="userId">The id of the user to generate the token for.</param>
  /// <returns>The generated verification token as a <see cref="Result{T}"/>.</returns>
  Task<Result<VerificationToken>> GenerateVerificationToken(string userId);

  /// <summary>
  /// Refreshes a user's access token.
  /// </summary>
  /// <param name="userId">The id of the user to refresh the token for.</param>
  /// <param name="refreshToken">The refresh token.</param>
  /// <returns>An instance of <see cref="Result{T}"/> containing the access token and refresh token.</returns>
  Task<Result<(string AccessToken, RefreshToken RefreshToken)>> RefreshAccessTokenAsync(string userId, string refreshToken);

  /// <summary>
  /// Removes all the invalid refresh tokens for a user.
  /// </summary>
  /// <param name="userId">The user id.</param>
  /// <returns>A <see cref="Task"/>.</returns>
  Task RemoveAllInvalidRefreshTokensAsync(string userId);

  /// <summary>
  /// Revokes a refresh token.
  /// </summary>
  /// <param name="userId">The user id.</param>
  /// <param name="refreshToken">The refresh token.</param>
  /// <returns>A <see cref="Task"/>.</returns>
  Task RevokeRefreshTokenAsync(string userId, string refreshToken);
}