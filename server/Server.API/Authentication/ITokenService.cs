
namespace Server.API.Authentication;

/// <summary>
/// A service for managing tokens.
/// </summary>
interface ITokenService
{
  string GenerateAccessToken(User existingUser);
  Task<Result<RefreshToken>> GenerateRefreshToken(string userId);
}