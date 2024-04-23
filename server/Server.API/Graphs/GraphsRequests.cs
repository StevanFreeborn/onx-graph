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
  [FromServices] IEncryptionService EncryptionService
);