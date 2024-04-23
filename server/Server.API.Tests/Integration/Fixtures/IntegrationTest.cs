namespace Server.API.Tests.Integration.Fixtures;

public class IntegrationTest : IClassFixture<TestServerFactory>
{
  protected readonly TestServerFactory _factory;
  protected readonly HttpClient _client;
  protected readonly MailHogService _mailHogService;
  internal readonly IEncryptionService EncryptionService;
  internal readonly CorsOptions CorsOptions;
  internal readonly MongoDbContext Context;

  public IntegrationTest(TestServerFactory factory)
  {
    _factory = factory;
    _client = _factory.CreateClient();
    _mailHogService = _factory.Services.GetRequiredService<MailHogService>();
    EncryptionService = _factory.Services.GetRequiredService<IEncryptionService>();
    CorsOptions = _factory.Services.GetRequiredService<IOptions<CorsOptions>>().Value;
    Context = _factory.Services.GetRequiredService<MongoDbContext>();
  }
}