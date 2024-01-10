
namespace Server.API.Authentication;

interface ITokenRepository
{
  Task<RefreshToken> CreateTokenAsync(RefreshToken token);
}