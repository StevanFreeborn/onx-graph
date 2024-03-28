namespace Server.API.Authentication;

/// <summary>
/// Represents the data needed to register
/// </summary>
record RegisterDto(string Email, string Password)
{
  internal RegisterDto() : this(string.Empty, string.Empty) { }

  internal User ToUser()
  {
    return new User
    {
      Email = Email,
      Password = Password
    };
  }
}

/// <summary>
/// Validator for <see cref="RegisterDto"/>
/// </summary>
class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
  public RegisterDtoValidator()
  {
    RuleFor(dto => dto.Email)
      .NotEmpty()
      .EmailAddress()
      .WithMessage("Email must be a valid email address.");

    // At least one uppercase letter - (?=.*[A-Z])
    // At least one lowercase letter - (?=.*[a-z])
    // At least one number - (?=.*\d)
    // At least one special character - (?=.*\W)
    // Must be at least 8 characters long - {8,}
    RuleFor(dto => dto.Password)
      .NotEmpty()
      .Matches(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*\W)[A-Za-z\d\W]{8,}$")
      .WithMessage("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number and one special character.");
  }
}

/// <summary>
/// Represents a request to register a user
/// </summary>
record RegisterRequest(
  [FromBody] RegisterDto Dto,
  [FromServices] IOptions<CorsOptions> CorsOptions,
  [FromServices] IValidator<RegisterDto> Validator,
  [FromServices] IUserService UserService,
  [FromServices] IEmailService EmailService,
  [FromServices] ILogger<RegisterRequest> Logger,
  [FromServices] ITokenService TokenService
);

/// <summary>
/// Represents the data needed to login
/// </summary>
record LoginDto(string Email, string Password)
{
  internal LoginDto() : this(string.Empty, string.Empty) { }
}

/// <summary>
/// Validator for <see cref="LoginDto"/>
/// </summary>
class LoginDtoValidator : AbstractValidator<LoginDto>
{
  public LoginDtoValidator()
  {
    RuleFor(dto => dto.Email)
      .NotEmpty()
      .EmailAddress()
      .WithMessage("Email must be a valid email address.");

    RuleFor(dto => dto.Password).NotEmpty();
  }
}

/// <summary>
/// Represents a login request
/// </summary>
record LoginRequest(
  HttpContext Context,
  [FromBody] LoginDto Dto,
  [FromServices] IValidator<LoginDto> Validator,
  [FromServices] IUserService UserService
);

/// <summary>
/// Represents logout request
/// </summary>
record LogoutRequest(
  HttpContext Context,
  [FromServices] ITokenService TokenService
);

/// <summary>
/// Represents a request to refresh a token
/// </summary>
record RefreshTokenRequest(
  HttpContext Context,
  [FromServices] ITokenService TokenService
);

/// <summary>
/// Represents the data needed to resend verification email
/// </summary>
record ResendVerificationEmailDto(string Email)
{
  internal ResendVerificationEmailDto() : this(string.Empty) { }
}

/// <summary>
/// Validator for <see cref="LoginDto"/>
/// </summary>
class ResendVerificationEmailDtoValidator : AbstractValidator<ResendVerificationEmailDto>
{
  public ResendVerificationEmailDtoValidator()
  {
    RuleFor(dto => dto.Email)
      .NotEmpty()
      .EmailAddress()
      .WithMessage("Email must be a valid email address.");
  }
}

/// <summary>
/// Represents a request to resend verification email
/// </summary>
record ResendVerificationEmailRequest(
  [FromBody] ResendVerificationEmailDto Dto,
  [FromServices] IValidator<ResendVerificationEmailDto> Validator,
  [FromServices] IUserService UserService,
  [FromServices] IEmailService EmailService,
  [FromServices] ITokenService TokenService,
  [FromServices] IOptions<CorsOptions> CorsOptions
);

/// <summary>
/// Represents the data needed to verify an account
/// </summary>
record VerifyAccountDto(string Token)
{
  internal VerifyAccountDto() : this(string.Empty) { }
}

/// <summary>
/// Validator for <see cref="VerifyAccountDto"/>
/// </summary>
class VerifyAccountDtoValidator : AbstractValidator<VerifyAccountDto>
{
  public VerifyAccountDtoValidator()
  {
    RuleFor(dto => dto.Token).NotEmpty();
  }
}

/// <summary>
/// Represents a request to verify an account
/// </summary>
record VerifyAccountRequest(
  [FromBody] VerifyAccountDto Dto,
  [FromServices] IValidator<VerifyAccountDto> Validator,
  [FromServices] IUserService UserService,
  [FromServices] ITokenService TokenService
);