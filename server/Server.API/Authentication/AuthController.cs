namespace Server.API.Authentication;

/// <summary>
/// Controller for handling authentication requests
/// </summary>
static class AuthController
{
  /// <summary>
  /// Registers a new user.
  /// </summary>
  /// <param name="req">The request as an <see cref="RegisterRequest"/> instance.</param>
  /// <returns>A <see cref="Task"/> of <see cref="IResult"/>.</returns>
  internal static async Task<IResult> Register([AsParameters] RegisterRequest req)
  {
    var validationResult = await req.Validator.ValidateAsync(req.Dto);

    if (validationResult.IsValid is false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var registrationResult = await req.UserService.RegisterUserAsync(req.Dto.ToUser());

    if (registrationResult.IsFailed)
    {
      return Results.Problem(
        title: "Registration failed",
        detail: "Unable to register user. See errors for details.",
        statusCode: (int)HttpStatusCode.Conflict,
        extensions: new Dictionary<string, object?> { { "Errors", registrationResult.Errors } }
      );
    }

    var tokenResult = await req.TokenService.GenerateVerificationToken(registrationResult.Value);

    if (tokenResult.IsFailed)
    {
      req.Logger.LogError("Failed to generate verification token for user: {UserId}", registrationResult.Value);
    }

    if (tokenResult.IsSuccess)
    {
      var emailMessage = BuildVerificationEmail(
        req.Dto.Email,
        tokenResult.Value.Token,
        req.CorsOptions.Value.AllowedOrigins[0]
      );

      var emailResult = await req.EmailService.SendEmailAsync(emailMessage);

      if (emailResult.IsFailed)
      {
        req.Logger.LogError("Failed to send verification email for user: {UserId}", registrationResult.Value);
      }
    }

    return Results.Created(
      uri: $"/users/{registrationResult.Value}",
      value: new RegisterUserResponse(registrationResult.Value)
    );
  }

  /// <summary>
  /// Logs in a user.
  /// </summary>
  /// <param name="req">The request as a <see cref="LoginRequest"/> instance.</param>
  /// <returns>A <see cref="Task"/> of <see cref="IResult"/>.</returns>
  internal static async Task<IResult> Login([AsParameters] LoginRequest req)
  {
    var validationResult = await req.Validator.ValidateAsync(req.Dto);

    if (validationResult.IsValid is false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var loginResult = await req.UserService.LoginUserAsync(req.Dto.Email, req.Dto.Password);

    var problemTitle = "Login failed";
    var problemDetail = "Unable to login user. See errors for details.";

    if (loginResult.IsFailed && loginResult.Errors.Exists(e => e is InvalidLoginError))
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.Unauthorized,
        extensions: new Dictionary<string, object?> { { "Errors", loginResult.Errors } }
      );
    }

    if (loginResult.IsFailed && loginResult.Errors.Exists(e => e is UserNotVerifiedError))
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.Forbidden,
        extensions: new Dictionary<string, object?> { { "Errors", loginResult.Errors } }
      );
    }

    req.Context.Response.SetRefreshTokenCookie(
      loginResult.Value.RefreshToken.Token,
      loginResult.Value.RefreshToken.ExpiresAt
    );

    return Results.Ok(new LoginUserResponse(loginResult.Value.AccessToken));
  }

  /// <summary>
  /// Logs out a user.
  /// </summary>
  /// <param name="req">The request as a <see cref="LogoutRequest"/> instance.</param>
  /// <returns>A <see cref="Task"/> of <see cref="IResult"/>.</returns>
  internal static async Task<IResult> Logout([AsParameters] LogoutRequest req)
  {
    var userId = req.Context.GetUserId();

    if (userId is null)
    {
      return Results.Problem(
        title: "Unable to logout user",
        detail: "No user is logged in",
        statusCode: 401
      );
    }

    var refreshToken = req.Context.Request.GetRefreshTokenCookie();

    if (string.IsNullOrWhiteSpace(refreshToken) is false)
    {
      await req.TokenService.RevokeRefreshTokenAsync(userId, refreshToken);
      await req.TokenService.RemoveAllInvalidRefreshTokensAsync(userId);
    }

    req.Context.Response.SetRefreshTokenCookie(string.Empty, DateTime.UtcNow.AddDays(-1));
    return Results.Ok();
  }

  /// <summary>
  /// Refreshes a user's access token.
  /// </summary>
  /// <param name="req">The request as a <see cref="RefreshTokenRequest"/> instance.</param>
  /// <returns>A <see cref="Task"/> of <see cref="IResult"/>.</returns>
  internal static async Task<IResult> RefreshToken([AsParameters] RefreshTokenRequest req)
  {
    var userId = req.Context.GetUserId();
    var refreshToken = req.Context.Request.GetRefreshTokenCookie();

    if (userId is null || string.IsNullOrWhiteSpace(refreshToken))
    {
      return Results.Unauthorized();
    }

    var refreshTokenResult = await req.TokenService.RefreshAccessTokenAsync(userId, refreshToken);

    if (refreshTokenResult.IsFailed)
    {
      req.Context.Response.SetRefreshTokenCookie(string.Empty, DateTime.UtcNow.AddDays(-1));
      return Results.Unauthorized();
    }

    req.Context.Response.SetRefreshTokenCookie(
      refreshTokenResult.Value.RefreshToken.Token,
      refreshTokenResult.Value.RefreshToken.ExpiresAt
    );

    return Results.Ok(new LoginUserResponse(refreshTokenResult.Value.AccessToken));
  }

  /// <summary>
  /// Resends a user's verification email.
  /// </summary>
  /// <param name="req">The request as a <see cref="ResendVerificationEmailRequest"/> instance.</param>
  /// <returns>A <see cref="Task"/> of <see cref="IResult"/>.</returns>
  internal static async Task<IResult> ResendVerificationEmail([AsParameters] ResendVerificationEmailRequest req)
  {
    var validationResult = await req.Validator.ValidateAsync(req.Dto);

    if (validationResult.IsValid is false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var userResult = await req.UserService.GetUserByEmailAsync(req.Dto.Email);

    var problemTitle = "Resend verification email failed";
    var problemDetail = "Unable to resend verification email. See errors for details.";

    if (userResult.IsFailed)
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", userResult.Errors } }
      );
    }

    if (userResult.Value.IsVerified)
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.Conflict,
        extensions: new Dictionary<string, object?> { { "Errors", new[] { new UserAlreadyVerifiedError(userResult.Value.Email) } } }
      );
    }

    await req.TokenService.RevokeUserVerificationTokensAsync(userResult.Value.Id);

    var tokenResult = await req.TokenService.GenerateVerificationToken(userResult.Value.Id);

    if (tokenResult.IsFailed)
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.InternalServerError,
        extensions: new Dictionary<string, object?> { { "Errors", tokenResult.Errors } }
      );
    }

    var emailMessage = BuildVerificationEmail(
      userResult.Value.Email,
      tokenResult.Value.Token,
      req.CorsOptions.Value.AllowedOrigins[0]
    );

    var emailResult = await req.EmailService.SendEmailAsync(emailMessage);

    if (emailResult.IsFailed)
    {
      return Results.Problem(
        title: "Resend verification email failed",
        detail: "Unable to resend verification email. See errors for details.",
        statusCode: (int)HttpStatusCode.InternalServerError,
        extensions: new Dictionary<string, object?> { { "Errors", emailResult.Errors } }
      );
    }

    return Results.NoContent();
  }

  /// <summary>
  /// Verifies a user's account.
  /// </summary>
  /// <param name="req">The request as a <see cref="VerifyAccountRequest"/> instance.</param>
  /// <returns>A <see cref="Task"/> of <see cref="IResult"/>.</returns>
  internal static async Task<IResult> VerifyAccount([AsParameters] VerifyAccountRequest req)
  {
    var validationResult = await req.Validator.ValidateAsync(req.Dto);

    if (validationResult.IsValid is false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var verifyTokenResult = await req.TokenService.VerifyVerificationTokenAsync(req.Dto.Token);

    var problemTitle = "Verification failed";
    var problemDetail = "Unable to verify account. See errors for details.";

    if (
      verifyTokenResult.IsFailed &&
      verifyTokenResult.Errors.Exists(e => e is TokenDoesNotExistError)
    )
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", verifyTokenResult.Errors } }
      );
    }

    if (
      verifyTokenResult.IsFailed &&
      verifyTokenResult.Errors.Exists(e => e is ExpiredTokenError or InvalidTokenError)
    )
    {
      await req.TokenService.RevokeVerificationTokenAsync(req.Dto.Token);

      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.BadRequest,
        extensions: new Dictionary<string, object?> { { "Errors", verifyTokenResult.Errors } }
      );
    }

    var verifyUserResult = await req.UserService.VerifyUserAsync(verifyTokenResult.Value.UserId);

    if (verifyUserResult.IsFailed && verifyUserResult.Errors.Exists(e => e is UserDoesNotExistError))
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", verifyUserResult.Errors } }
      );
    }

    if (verifyUserResult.IsFailed && verifyUserResult.Errors.Exists(e => e is UserAlreadyVerifiedError))
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.Conflict,
        extensions: new Dictionary<string, object?> { { "Errors", verifyUserResult.Errors } }
      );
    }

    await req.TokenService.RevokeVerificationTokenAsync(req.Dto.Token);
    await req.TokenService.RemoveAllInvalidVerificationTokensAsync(verifyTokenResult.Value.UserId);

    return Results.NoContent();
  }

  private static EmailMessage BuildVerificationEmail(string email, string token, string origin) =>
    new()
    {
      To = email,
      Subject = "Welcome to OnxGraph! Verify your account to get started.",
      HtmlContent = $"""
          <h1>Welcome to OnxGraph!</h1>
          <p>We're excited to welcome you to OnxGraph! Before you begin we need to verify your account. Follow these steps to complete the verification process:</p>
          <p>Click the link below to verify your account:</p>
          <a href='{origin}/masses/verify-account?t={token}'>Verify Account</a>
          <p>If you didn't create an account with OnxGraph, please ignore this email.</p>
        """
    };
}