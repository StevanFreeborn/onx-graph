namespace Server.API.Swagger;

class ApiVersionOperationFilter : IOperationFilter
{
  public void Apply(OpenApiOperation operation, OperationFilterContext context)
  {
    operation.Parameters ??= [];

    var version = context.DocumentName.ToLowerInvariant().Replace("v", "");

    operation.Parameters.Add(new OpenApiParameter
    {
      In = ParameterLocation.Header,
      Name = "x-api-version",
      Description = "Versioning header",
      Schema = new()
      {
        Type = "string",
        Default = new OpenApiString(version)
      },
    });
  }
}