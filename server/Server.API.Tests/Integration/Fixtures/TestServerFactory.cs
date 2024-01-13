using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        options.Audience = TestJwtTokenBuilder.TestJwtAudience;
        options.Issuer = TestJwtTokenBuilder.TestJwtIssuer;
        options.ExpiryInMinutes = TestJwtTokenBuilder.TestJwtExpiryInMinutes;
      });
      services.Configure<MongoDbOptions>(options =>
      {
        options.ConnectionString = _container.GetConnectionString();
        options.DatabaseName = "tests";
      });
    });

    builder.ConfigureServices(
      services =>
        services.PostConfigure<JwtBearerOptions>(
          JwtBearerDefaults.AuthenticationScheme,
          options =>
            options.TokenValidationParameters = new()
            {
              ValidIssuer = TestJwtTokenBuilder.TestJwtIssuer,
              ValidAudience = TestJwtTokenBuilder.TestJwtAudience,
              IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(TestJwtTokenBuilder.TestJwtSecret)
              ),
            }
        )
    );
  }

  new public async Task DisposeAsync()
  {
    await _container.DisposeAsync();
  }
}