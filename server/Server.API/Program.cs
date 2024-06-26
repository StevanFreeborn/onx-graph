using Microsoft.AspNetCore.RateLimiting;

Log.Logger = new LoggerConfiguration()
  .WriteTo.Console()
  .CreateBootstrapLogger();

Log.Information("Starting up the application");

try
{
  var builder = WebApplication.CreateBuilder(args);
  var config = builder.Configuration;
  config.AddEnvironmentVariables();

  // add serilog
  builder.Host.UseSerilog((context, loggerConfiguration) =>
  {
    loggerConfiguration
      .MinimumLevel.Debug()
      .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
      .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
      .Enrich.FromLogContext()
      .Enrich.WithMachineName()
      .Enrich.WithThreadId()
      .WriteTo.File(
        new CompactJsonFormatter(),
        "logs/logs.json",
        rollingInterval: RollingInterval.Day
      );

    var lokiUrl = context.Configuration.GetValue<string>("LOKI_URL");

    if (string.IsNullOrEmpty(lokiUrl) is false)
    {
      loggerConfiguration
        .WriteTo
        .GrafanaLoki(
          lokiUrl,
          labels: [new() { Key = "app", Value = "OnxGraph.Server.API" }],
          propertiesAsLabels: ["app"]
        );
    }
  }, preserveStaticLogger: true);


  // add problem details service
  builder.Services.AddProblemDetails();


  // add versioning
  var versionOne = new ApiVersion(1, 0);

  builder.Services.AddApiVersioning(opts =>
  {
    opts.DefaultApiVersion = versionOne;
    opts.AssumeDefaultVersionWhenUnspecified = true;
    opts.ReportApiVersions = true;
    opts.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
  });

  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen(opts =>
  {
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
      In = ParameterLocation.Header,
      Description = "Enter 'Bearer' [space] and then your token in the text input below.",
      Name = "Authorization",
      Type = SecuritySchemeType.Http,
      BearerFormat = "JWT",
      Scheme = "Bearer"
    });

    opts.OperationFilter<AuthorizeOperationFilter>();

    opts.OperationFilter<ApiVersionOperationFilter>();
  });


  // add authentication and authorization
  var jwtOptions = new JwtOptions();
  config.GetSection(nameof(JwtOptions)).Bind(jwtOptions);

  builder.Services.ConfigureOptions<JwtOptionsSetup>();

  builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
      o.TokenValidationParameters = new TokenValidationParameters
      {
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(jwtOptions.Secret)
        ),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromSeconds(0),
      };

      o.Events = new JwtBearerEvents()
      {
        OnMessageReceived = context =>
        {
          var accessToken = context.Request.Query["access_token"];

          var path = context.HttpContext.Request.Path;

          if (string.IsNullOrEmpty(accessToken) is false && path.StartsWithSegments("/graphs/hub"))
          {
            context.Token = accessToken;
          }

          return Task.CompletedTask;
        }
      };
    })
    .AddJwtBearer(
      "AllowExpiredToken",
      o => o.TokenValidationParameters = new TokenValidationParameters
      {
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(jwtOptions.Secret)
        ),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromSeconds(0),
      }
    );

  builder.Services.AddAuthorization();

  builder.Services.AddScoped<ITokenRepository, MongoTokenRepository>();
  builder.Services.AddScoped<ITokenService, TokenService>();
  builder.Services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
  builder.Services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
  builder.Services.AddScoped<IValidator<ResendVerificationEmailDto>, ResendVerificationEmailDtoValidator>();
  builder.Services.AddScoped<IValidator<VerifyAccountDto>, VerifyAccountDtoValidator>();

  // add cors
  var corsOptions = new CorsOptions();
  config.GetSection(nameof(CorsOptions)).Bind(corsOptions);
  builder.Services.ConfigureOptions<CorsOptionsSetup>();

  builder.Services.AddCors(
    options => options.AddPolicy(
      "CORSpolicy",
      policy => policy
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithOrigins(corsOptions.AllowedOrigins)
        .WithExposedHeaders("X-Retry-After")
    )
  );

  // add mongo db for persistence
  // add as singleton as client should be reused
  builder.Services.ConfigureOptions<MongoDbOptionsSetup>();
  builder.Services.AddSingleton<MongoDbContext>();
  builder.Services.AddHostedService<MongoIndexService>();

  // add identity to dependency injection
  // add as scoped as we want a new instance per request
  builder.Services.AddScoped<IUserRepository, MongoUserRepository>();
  builder.Services.AddScoped<IUserService, UserService>();
  builder.Services.AddHostedService<AccountMonitor>();


  // add time provider
  builder.Services.AddSingleton(TimeProvider.System);


  // add email service
  builder.Services.ConfigureOptions<SmtpOptionsSetup>();
  builder.Services.AddTransient<IEmailClient, SmtpEmailClient>();
  builder.Services.AddScoped<IEmailService, DotNetEmailService>();


  // add encryption service
  builder.Services.ConfigureOptions<EncryptionOptionsSetup>();
  builder.Services.AddSingleton<IEncryptionService, EncryptionService>();


  // add graph service
  builder.Services.AddScoped<IValidator<AddGraphDto>, AddGraphDtoValidator>();
  builder.Services.AddScoped<IValidator<GraphDto>, GraphDtoValidator>();
  builder.Services.AddScoped<IValidator<UpdateGraphKeyDto>, UpdateGraphKeyDtoValidator>();
  builder.Services.AddScoped<IGraphRepository, MongoGraphRepository>();
  builder.Services.AddScoped<IGraphService, GraphService>();
  builder.Services.AddSingleton<IGraphQueue, ChannelGraphQueue>();
  builder.Services.AddSingleton<IGraphProcessor, GraphProcessor>();
  builder.Services.AddHostedService<GraphQueueService>();
  builder.Services.AddSignalR();
  builder.Services
    .AddHttpClient(
      OnspringClientFactory.HttpClientName,
      client => client.BaseAddress = new Uri("https://api.onspring.com")
    )
    .AddStandardResilienceHandler();

  builder.Services.AddSingleton<IOnspringClientFactory, OnspringClientFactory>();

  // add rate limiting and whitelist client origin
  builder.Services.AddRateLimiter(options =>
  {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = (context, rateLimit) =>
    {
      if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
      {
        var retryDate = DateTime.UtcNow.Add(retryAfter);
        context.HttpContext.Response.Headers.Append("X-Retry-After", retryDate.ToString("o"));
      }

      return ValueTask.CompletedTask;
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
      httpContext =>
      {
        var partitionKey = httpContext.User.Identity?.Name
          ?? httpContext.Request.Headers["X-Forwarded-For"].ToString()
          ?? httpContext.Connection.RemoteIpAddress?.ToString()
          ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
          partitionKey: partitionKey,
          factory: partition => new FixedWindowRateLimiterOptions
          {
            PermitLimit = 10000,
            QueueLimit = 0,
            Window = TimeSpan.FromMinutes(1)
          }
        );
      }
    );

    options.AddPolicy(
      RateLimitingPolicy.Fixed,
      httpContext =>
      {
        var partitionKey = httpContext.User.Identity?.Name
          ?? httpContext.Request.Headers["X-Forwarded-For"].ToString()
          ?? httpContext.Connection.RemoteIpAddress?.ToString()
          ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
          partitionKey: partitionKey,
          factory: partition => new FixedWindowRateLimiterOptions
          {
            PermitLimit = 10,
            QueueLimit = 0,
            Window = TimeSpan.FromMinutes(1)
          }
        );
      }
    );
  });

  // build the app
  Log.Information("Building the application");

  var app = builder.Build();


  // add request logging middleware
  app.UseSerilogRequestLogging();


  // add problem details middleware
  app.UseStatusCodePages();


  // add error handling middleware
  app.UseMiddleware<ErrorMiddleware>();


  // add swagger when developing
  if (app.Environment.IsDevelopment())
  {
    app.UseSwagger();
    app.UseSwaggerUI();
  }


  // add https redirection
  app.UseHttpsRedirection();


  // build the api version set
  var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();


  // map endpoints
  app
    .MapVersionOneAuthEndpoints()
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(versionOne);

  app
    .MapVersionOneUsersEndpoints()
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(versionOne);

  app
    .MapVersionOneGraphsEndpoints()
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(versionOne);

  // use cors
  app.UseCors("CORSpolicy");


  // add rate limiting middleware
  if (app.Environment.IsProduction())
  {
    app.UseRateLimiter();
  }


  // wire up authentication and authorization
  app.UseAuthentication();
  app.UseAuthorization();


  // run the app
  Log.Information("Running the application");

  await app.RunMigrations();
  app.Run();

  Log.Information("Shutting down the application");
}
catch (Exception ex)
{
  Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}


/// <summary>
/// The main entry point for the application.
/// </summary>
public partial class Program { }