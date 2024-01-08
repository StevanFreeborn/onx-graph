namespace Server.API.Identity;

/// <summary>
/// A service for managing users.
/// </summary>
interface IUserService
{
  /// <summary>
  /// Registers a new user.
  /// </summary>
  /// <param name="user">The user to register.</param>
  /// <returns>A <see cref="Result{T}"/> indicating the outcome of the operation.</returns>
  Task<Result<string>> RegisterUserAsync(User user);
}