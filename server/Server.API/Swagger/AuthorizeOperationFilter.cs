namespace Server.API.Swagger;

class AuthorizeOperationFilter : IOperationFilter
{
  public void Apply(OpenApiOperation operation, OperationFilterContext context)
  {
    var authAttributes = context.MethodInfo.DeclaringType!.GetCustomAttributes(true)
      .Union(context.MethodInfo.GetCustomAttributes(true))
      .Union(context.ApiDescription.ActionDescriptor.EndpointMetadata)
      .OfType<AuthorizeAttribute>();

    if (authAttributes.Any() == false)
    {
      return;
    }

    operation.Security =
      [
        new OpenApiSecurityRequirement
        {
          [
            new()
            {
              Reference = new()
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              }
            }
          ] = []
        }
      ];
  }
}