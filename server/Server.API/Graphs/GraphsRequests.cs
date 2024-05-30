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

/// <summary>
/// Represents a request to get a graph key
/// </summary>
record GetGraphKeyRequest(
  HttpContext HttpContext,
  [FromRoute] string Id,
  [FromServices] IGraphService GraphService,
  [FromServices] IUserService UserService,
  [FromServices] IEncryptionService EncryptionService
);

/// <summary>
/// Represents a request to delete a graph
/// </summary>
record DeleteGraphRequest(
  HttpContext HttpContext,
  [FromRoute] string Id,
  [FromServices] IUserService UserService,
  [FromServices] IGraphService GraphService
);

/// <summary>
/// Validator for <see cref="GraphDto"/>
/// </summary>
class GraphDtoValidator : AbstractValidator<GraphDto>
{
  public GraphDtoValidator()
  {
    RuleFor(dto => dto.Name).NotEmpty();
  }
}


/// <summary>
/// Represents a request to update a graph
/// </summary>
record UpdateGraphRequest(
  HttpContext HttpContext,
  [FromRoute] string Id,
  [FromBody] GraphDto Dto,
  [FromServices] IValidator<GraphDto> Validator,
  [FromServices] IGraphService GraphService,
  [FromServices] IUserService UserService
);

/// <summary>
/// Represents the data needed to update a graph key
/// </summary>
record UpdateGraphKeyDto(string Key);

/// <summary>
/// Validator for <see cref="UpdateGraphKeyDto"/>
/// </summary>
class UpdateGraphKeyDtoValidator : AbstractValidator<UpdateGraphKeyDto>
{
  public UpdateGraphKeyDtoValidator()
  {
    RuleFor(dto => dto.Key).NotEmpty();
  }
}


/// <summary>
/// Represents a request to update a graph key
/// </summary>
record UpdateGraphKeyRequest(
  HttpContext HttpContext,
  [FromRoute] string Id,
  [FromBody] UpdateGraphKeyDto Dto,
  [FromServices] IValidator<UpdateGraphKeyDto> Validator,
  [FromServices] IGraphService GraphService,
  [FromServices] IUserService UserService,
  [FromServices] IEncryptionService EncryptionService
);