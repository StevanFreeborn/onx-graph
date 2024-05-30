namespace Server.API.Identity;

/// <summary>
/// A repository for managing users.
/// </summary>
interface IUserRepository
{
  /// <summary>
  /// Creates a new user.
  /// </summary>
  /// <param name="user">The user to create.</param>
  /// <returns>The created user as an <see cref="User"/> instance.</returns>
  Task<User> CreateUserAsync(User user);

  /// <summary>
  /// Gets a user by their email address.
  /// </summary>
  /// <param name="email">The email address of the user to get.</param>
  /// <returns>The user as an <see cref="User"/> instance.</returns>
  Task<User?> GetUserByEmailAsync(string email);

  /// <summary>
  /// Gets a user by their id.
  /// </summary>
  /// <param name="userId">The id of the user to get.</param>
  /// <returns>The user as an <see cref="User"/> instance.</returns>
  Task<User?> GetUserByIdAsync(string userId);

  /// <summary>
  /// Gets a user by their username.
  /// </summary>
  /// <param name="username">The username of the user to get.</param>
  /// <returns>The user as an <see cref="User"/> instance.</returns>
  Task<User?> GetUserByUsernameAsync(string username);

  /// <summary>
  /// Updates a user.
  /// </summary>
  /// <param name="existingUser">The user to update.</param>
  /// <returns>A <see cref="Task"/>.</returns>
  Task UpdateUserAsync(User existingUser);
}