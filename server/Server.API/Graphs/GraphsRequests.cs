namespace Server.API.Graphs;

/// <summary>
/// Represents the data needed to add a graph
/// </summary>
record AddGraphDto(string Name, string ApiKey)
{
  internal AddGraphDto() : this(string.Empty, string.Empty) { }
}

/// <summary>
/// Validator for <see cref="AddGraphDto"/>
/// </summary>
class AddGraphDtoValidator : AbstractValidator<AddGraphDto>
{
  public AddGraphDtoValidator()
  {
    RuleFor(dto => dto.Name).NotEmpty();
    RuleFor(dto => dto.ApiKey).NotEmpty();
  }
}

/// <summary>
/// Represents a request to add a graph
/// </summary>
record AddGraphRequest(
  HttpContext HttpContext,
  [FromBody] AddGraphDto Dto,
  [FromServices] IValidator<AddGraphDto> Validator,
  [FromServices] IGraphService GraphService,
  [FromServices] IUserService UserService,
  [FromServices] IEncryptionService EncryptionService,
  [FromServices] IGraphQueue GraphQueue
);

/// <summary>
/// Represents a request to get graphs
/// </summary>
record GetGraphsRequest(
  HttpContext HttpContext,
  [FromServices] IGraphService GraphService,
  [FromQuery] int PageNumber = 1,
  [FromQuery] int PageSize = 10
);

/// <summary>
/// Represents a request to get a graph
/// </summary>
record GetGraphRequest(
  HttpContext HttpContext,
  [FromRoute] string Id,
  [FromServices] IGraphService GraphService
);