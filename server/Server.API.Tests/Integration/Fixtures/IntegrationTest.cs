namespace Server.API.Tests.Integration.Fixtures;

public class IntegrationTest : IClassFixture<TestServerFactory>
{
  protected readonly TestServerFactory _factory;
  protected readonly HttpClient _client;
  internal readonly MongoDbContext context;

  public IntegrationTest(TestServerFactory factory)
  {
    _factory = factory;
    _client = _factory.CreateClient();
    context = _factory.Services.GetRequiredService<MongoDbContext>();
  }
}