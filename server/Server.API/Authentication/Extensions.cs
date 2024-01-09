namespace Server.API.Authentication;

/// <summary>
/// Extensions for <see cref="HttpResponse"/>
/// </summary>
static class HttpResponseExtensions
{
  /// <summary>
  /// Sets the refresh token cookie
  /// </summary>
  /// <param name="response">The <see cref="HttpResponse"/> instance</param>
  /// <param name="token">The refresh token</param>
  /// <param name="expiresAt">The expiration date of the refresh token</param>
  internal static void SetRefreshTokenCookie(this HttpResponse response, string token, DateTime expiresAt)
  {
    response.Cookies.Append(
      "onxRefreshToken",
      token,
      new CookieOptions
      {
        HttpOnly = true,
        Expires = expiresAt,
        SameSite = SameSiteMode.None,
        Secure = true
      }
    );
  }
}