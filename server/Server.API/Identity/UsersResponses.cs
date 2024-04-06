record UserResponse(string Id, string Username)
{
  internal UserResponse(User user) : this(user.Id, user.Username) { }
}