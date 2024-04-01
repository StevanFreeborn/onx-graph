namespace Server.API.Tests.Integration.Fixtures;

public class TestServerFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder().Build();
  private readonly IContainer _mailHogContainer = new ContainerBuilder()
    .WithImage("mailhog/mailhog")
    .WithPortBinding(1025, true)
    .WithPortBinding(8025, true)
    .Build();

  public async Task InitializeAsync()
  {
    await _mongoDbContainer.StartAsync();
    await _mailHogContainer.StartAsync();
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
        options.ConnectionString = _mongoDbContainer.GetConnectionString();
        options.DatabaseName = "tests";
      });

      services.Configure<SmtpOptions>(options =>
      {
        options.SmtpAddress = _mailHogContainer.Hostname;
        options.SmtpPort = _mailHogContainer.GetMappedPublicPort(1025);
        options.SenderEmail = "onxGraphTesting@test.com";
        options.SenderPassword = string.Empty;
      });

      services.Configure<CorsOptions>(options =>
      {
        options.AllowedOrigins = ["https://localhost:3001"];
      });

      var mailHogBaseUrl = $"http://{_mailHogContainer.Hostname}:{_mailHogContainer.GetMappedPublicPort(8025)}";

      services.AddHttpClient<MailHogService>(
        client => client.BaseAddress = new Uri(mailHogBaseUrl)
      );
    });

    builder.ConfigureServices(
      services =>
      {
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
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateLifetime = true,
              ValidateIssuerSigningKey = true,
              ClockSkew = TimeSpan.FromSeconds(0),
            }
        );

        services.PostConfigure<JwtBearerOptions>(
          "AllowExpiredToken",
          options =>
            options.TokenValidationParameters = new()
            {
              ValidIssuer = TestJwtTokenBuilder.TestJwtIssuer,
              ValidAudience = TestJwtTokenBuilder.TestJwtAudience,
              IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(TestJwtTokenBuilder.TestJwtSecret)
              ),
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateLifetime = false,
              ValidateIssuerSigningKey = true,
              ClockSkew = TimeSpan.FromSeconds(0),
            }
        );
      });
  }

  new public async Task DisposeAsync()
  {
    await _mongoDbContainer.DisposeAsync();
    await _mailHogContainer.DisposeAsync();
  }
}