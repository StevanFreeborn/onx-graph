namespace Server.API.Authentication;

/// <summary>
/// Represents a verification token
/// </summary>
record VerificationToken : BaseToken
{
  internal VerificationToken() : base()
  {
    TokenType = TokenType.Verification;
  }
}