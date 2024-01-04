
using FluentResults;

namespace Server.API.Identity;

interface IUserService
{
  Task<Result<string>> RegisterUserAsync(User user);
}