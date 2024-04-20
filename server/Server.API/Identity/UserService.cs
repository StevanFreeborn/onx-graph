namespace Server.API.Identity;

/// <summary>
/// A service for managing users.
/// </summary>
/// <inheritdoc cref="IUserService"/>
class UserService(
  ITokenService tokenService,
  IUserRepository userRepository,
  ILogger<UserService> logger,
  IEncryptionService encryptionService
) : IUserService
{
  private readonly ITokenService _tokenService = tokenService;
  private readonly IUserRepository _userRepository = userRepository;
  private readonly ILogger<UserService> _logger = logger;
  private readonly IEncryptionService _encryptionService = encryptionService;

  public async Task<Result<User>> GetUserByEmailAsync(string userEmail)
  {
    var existingUser = await _userRepository.GetUserByEmailAsync(userEmail);

    if (existingUser is null)
    {
      return Result.Fail(new UserDoesNotExistError(userEmail));
    }

    return Result.Ok(existingUser);
  }

  public async Task<Result<User>> GetUserByIdAsync(string userId)
  {
    var existingUser = await _userRepository.GetUserByIdAsync(userId);

    if (existingUser is null)
    {
      return Result.Fail(new UserDoesNotExistError(userId));
    }

    return Result.Ok(existingUser);
  }

  public async Task<Result<(string AccessToken, RefreshToken RefreshToken)>> LoginUserAsync(string email, string password)
  {
    var existingUser = await _userRepository.GetUserByEmailAsync(email);

    if (existingUser is null)
    {
      return Result.Fail(new InvalidLoginError());
    }

    var passwordValid = BCrypt.Net.BCrypt.Verify(password, existingUser.Password);

    if (passwordValid is false)
    {
      return Result.Fail(new InvalidLoginError());
    }

    if (existingUser.IsVerified is false)
    {
      return Result.Fail(new UserNotVerifiedError(existingUser.Id));
    }

    var accessToken = _tokenService.GenerateAccessToken(existingUser);
    var refreshTokenResult = await _tokenService.GenerateRefreshToken(existingUser.Id);

    if (refreshTokenResult.IsFailed)
    {
      _logger.LogError("Failed to generate refresh token: {Errors}", refreshTokenResult.Errors);
      return Result.Fail(new LoginFailedError());
    }

    return Result.Ok((accessToken, refreshTokenResult.Value));
  }

  public async Task<Result<string>> RegisterUserAsync(User user)
  {
    var existingUser = await _userRepository.GetUserByEmailAsync(user.Email);

    if (existingUser is not null)
    {
      return Result.Fail(new UserAlreadyExistError(user.Email));
    }

    var username = await GenerateUniqueUsernameAsync(user.Email);
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

    user.Username = username;
    user.Password = passwordHash;

    var encryptionKey = _encryptionService.GenerateKey();
    var encryptedKey = await _encryptionService.Encrypt(encryptionKey);
    user.EncryptionKey = encryptedKey;

    var createdUser = await _userRepository.CreateUserAsync(user);

    return Result.Ok(createdUser.Id);
  }

  public async Task<Result> VerifyUserAsync(string userId)
  {
    var existingUser = await _userRepository.GetUserByIdAsync(userId);

    if (existingUser is null)
    {
      return Result.Fail(new UserDoesNotExistError(userId));
    }

    if (existingUser.IsVerified)
    {
      return Result.Fail(new UserAlreadyVerifiedError(userId));
    }

    existingUser.IsVerified = true;

    await _userRepository.UpdateUserAsync(existingUser);

    return Result.Ok();
  }

  private async Task<string> GenerateUniqueUsernameAsync(string email)
  {
    var random = new Random();
    var randomNumber = random.Next(0, 1000);
    var username = $"{email.Split('@')[0]}{randomNumber}";
    var existingUser = await _userRepository.GetUserByUsernameAsync(username);

    if (existingUser is not null)
    {
      _logger.LogInformation($"Username {username} already exists. Generating another.");
      return await GenerateUniqueUsernameAsync(email);
    }

    return username;
  }
}