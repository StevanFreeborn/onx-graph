var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Host.UseSerilog(
  (context, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(config)
    .Enrich.FromLogContext()
);

var versionOne = new ApiVersion(1, 0);

builder.Services.AddApiVersioning(opts =>
{
  opts.DefaultApiVersion = versionOne;
  opts.AssumeDefaultVersionWhenUnspecified = true;
  opts.ReportApiVersions = true;
  opts.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var versionSet = app.NewApiVersionSet()
  .HasApiVersion(new ApiVersion(1, 0))
  .Build();

app
  .MapGet("/", (ILogger<Program> logger) =>
  {
    logger.LogInformation("Hello World!");
    return "Hello World!";
  })
  .WithOpenApi(o => new(o)
  {
    Summary = "Get the root",
  })
  .WithApiVersionSet(versionSet)
  .MapToApiVersion(versionOne);

app.Run();