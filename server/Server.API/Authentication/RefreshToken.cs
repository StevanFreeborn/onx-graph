namespace Server.API.Authentication;

/// <summary>
/// Represents a refresh token
/// </summary>
record RefreshToken : BaseToken
{
  internal RefreshToken() : base()
  {
    TokenType = TokenType.Refresh;
  }
}