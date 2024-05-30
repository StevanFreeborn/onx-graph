namespace Server.API.Authentication;

/// <summary>
/// Represents a response to a successful registration
/// </summary>
record RegisterUserResponse(string Id);

/// <summary>
/// Represents a response to a successful login
/// </summary>
record LoginUserResponse(string AccessToken);