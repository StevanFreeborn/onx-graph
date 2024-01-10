var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;


// add logging
builder.Host.UseSerilog(
  (context, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(config)
    .Enrich.FromLogContext()
);


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
});


// add authentication and authorization
var jwtOptions = new JwtOptions();
config.GetSection(nameof(JwtOptions)).Bind(jwtOptions);

builder.Services.ConfigureOptions<JwtOptionsSetup>();

builder.Services
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(
    o => o.TokenValidationParameters = new TokenValidationParameters
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
    }
  );

builder.Services.AddAuthorization();
builder.Services.AddScoped<ITokenRepository, MongoTokenRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
builder.Services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();


// add mongo db for persistence
// add as singleton as client should be reused
builder.Services.ConfigureOptions<MongoDbOptionsSetup>();
builder.Services.AddSingleton<MongoDbContext>();


// add identity to dependency injection
// add as scoped as we want a new instance per request
builder.Services.AddScoped<IUserRepository, MongoUserRepository>();
builder.Services.AddScoped<IUserService, UserService>();


// add time provider
builder.Services.AddSingleton(TimeProvider.System);


// build the app
var app = builder.Build();


// add request logging middleware
app.UseSerilogRequestLogging();


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
  .Build();


// map endpoints
app.MapAuthEndpoints();

// wire up authentication and authorization
app.UseAuthentication();
app.UseAuthorization();


// run the app
app.Run();


/// <summary>
/// The main entry point for the application.
/// </summary>
public partial class Program { }