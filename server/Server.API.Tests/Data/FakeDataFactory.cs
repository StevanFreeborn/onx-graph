namespace Server.API.Tests.Data;

public static class FakeDataFactory
{
  internal static readonly Faker<User> TestUser = new Faker<User>()
    .RuleFor(u => u.Id, f => ObjectId.GenerateNewId().ToString())
    .RuleFor(u => u.Email, f => f.Person.Email)
    .RuleFor(u => u.Username, f => f.Person.UserName)
    .RuleFor(u => u.Password, "Password123!");
}