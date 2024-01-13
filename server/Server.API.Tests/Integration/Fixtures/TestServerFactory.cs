using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Server.API.Tests.Integration.Fixtures;

public class TestServerFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private readonly MongoDbContainer _container = new MongoDbBuilder().Build();

  public async Task InitializeAsync()
  {
    await _container.StartAsync();
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureAppConfiguration((context, config) =>
    {
      var projectDir = Directory.GetCurrentDirectory();
      var testConfig = Path.Combine(projectDir, "integration.tests.settings.json");
      config.AddJsonFile(testConfig);
    });

    builder.ConfigureTestServices(services =>
    {
      services.Configure<JwtOptions>(options =>
      {
        options.Secret = TestJwtTokenBuilder.TestJwtSecret;
        options.Audience = "TestAudience";
        options.Issuer = "TestIssuer";
        options.ExpiryInMinutes = 5;
      });
      services.Configure<MongoDbOptions>(options =>
      {
        options.ConnectionString = _container.GetConnectionString();
        options.DatabaseName = "tests";
      });
    });
  }

  new public async Task DisposeAsync()
  {
    await _container.DisposeAsync();
  }
}