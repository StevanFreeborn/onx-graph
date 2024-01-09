
namespace Server.API.Identity;

/// <summary>
/// A service for managing users.
/// </summary>
interface IUserService
{
  /// <summary>
  /// Logs in a user.
  /// </summary>
  /// <param name="email">The email of the user to log in.</param>
  /// <param name="password">The password of the user to log in.</param>
  /// <returns>A <see cref="Result{T}"/> indicating the outcome of the operation.</returns>
  Task<Result<(string AccessToken, RefreshToken RefreshToken)>> LoginUserAsync(string email, string password);

  /// <summary>
  /// Registers a new user.
  /// </summary>
  /// <param name="user">The user to register.</param>
  /// <returns>A <see cref="Result{T}"/> indicating the outcome of the operation.</returns>
  Task<Result<string>> RegisterUserAsync(User user);
}