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

/// <summary>
/// Extensions for <see cref="HttpRequest"/>
/// </summary>
static class HttpRequestExtensions
{
  /// <summary>
  /// Gets the refresh token cookie
  /// </summary>
  /// <param name="request">The <see cref="HttpRequest"/> instance</param>
  /// <returns>The refresh token cookie</returns>
  internal static string? GetRefreshTokenCookie(this HttpRequest request)
  {
    return request.Cookies["onxRefreshToken"];
  }
}

/// <summary>
/// Extensions for <see cref="HttpContext"/>
/// </summary>
static class HttpContextExtensions
{
  /// <summary>
  /// Gets the user id from the <see cref="HttpContext"/>
  /// </summary>
  /// <param name="context">The <see cref="HttpContext"/> instance</param>
  /// <returns>The user id</returns>
  internal static string? GetUserId(this HttpContext context)
  {
    return context.User.FindFirstValue(ClaimTypes.NameIdentifier);
  }
}