using ValidationFailure = FluentValidation.Results.ValidationFailure;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Server.API.Tests.Unit;

public class GraphsControllerTests
{
  private readonly Mock<HttpContext> _contextMock = new();
  private readonly Mock<IValidator<AddGraphDto>> _addGraphDtoValidatorMock = new();
  private readonly Mock<IValidator<GraphDto>> _graphDtoValidatorMock = new();
  private readonly Mock<IValidator<UpdateGraphKeyDto>> _updateGraphKeyDtoValidatorMock = new();
  private readonly Mock<IGraphService> _graphServiceMock = new();
  private readonly Mock<IUserService> _userServiceMock = new();
  private readonly Mock<IEncryptionService> _encryptionServiceMock = new();
  private readonly Mock<IGraphQueue> _graphQueueMock = new();

  private AddGraphRequest CreateAddGraphRequest(AddGraphDto dto) => new(
    _contextMock.Object,
    dto,
    _addGraphDtoValidatorMock.Object,
    _graphServiceMock.Object,
    _userServiceMock.Object,
    _encryptionServiceMock.Object,
    _graphQueueMock.Object
  );

  [Fact]
  public async Task AddGraph_WhenCalledByUnauthenticatedUser_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal());

    var mockDto = Mock.Of<AddGraphDto>();
    var request = CreateAddGraphRequest(mockDto);

    var result = await GraphsController.AddGraph(request);

    result.Should()
      .BeOfType<UnauthorizedHttpResult>();

    result.As<UnauthorizedHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status401Unauthorized);
  }

  [Fact]
  public async Task AddGraph_WhenCalledWithoutName_ItShouldReturnValidationProblemDetailsWith400StatusCode()
  {
    var expectedNameKey = "Name";
    var expectedNameErrorMessage = "The Name field is required.";

    var (_, user) = FakeDataFactory.TestUser.Generate();

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, user.Id)
      ])));

    var mockDto = Mock.Of<AddGraphDto>(dto => dto.Name == null);
    var validationResult = new ValidationResult(
      new[]
      {
        new ValidationFailure(expectedNameKey, expectedNameErrorMessage)
      }
    );

    _addGraphDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<AddGraphDto>(), default))
      .ReturnsAsync(validationResult);

    var request = CreateAddGraphRequest(mockDto);

    var result = await GraphsController.AddGraph(request);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be((int)HttpStatusCode.BadRequest);

    var validationProblemDetails = result.As<ProblemHttpResult>()
      .ProblemDetails as HttpValidationProblemDetails;

    validationProblemDetails?.Errors
      .Should()
      .ContainKey(expectedNameKey);

    validationProblemDetails?.Errors[expectedNameKey]
      .Should()
      .Contain(expectedNameErrorMessage);
  }

  [Fact]
  public async Task AddGraph_WhenCalledWithoutApiKey_ItShouldReturnValidationProblemDetailsWith400StatusCode()
  {
    var expectedApiKeyKey = "ApiKey";
    var expectedApiKeyErrorMessage = "The ApiKey field is required.";

    var (_, user) = FakeDataFactory.TestUser.Generate();

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, user.Id)
      ])));


    var mockDto = Mock.Of<AddGraphDto>(dto => dto.ApiKey == null);

    var validationResult = new ValidationResult(
      new[]
      {
        new ValidationFailure(expectedApiKeyKey, expectedApiKeyErrorMessage)
      }
    );

    _addGraphDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<AddGraphDto>(), default))
      .ReturnsAsync(validationResult);

    var request = CreateAddGraphRequest(mockDto);

    var result = await GraphsController.AddGraph(request);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be((int)HttpStatusCode.BadRequest);

    var validationProblemDetails = result.As<ProblemHttpResult>()
      .ProblemDetails as HttpValidationProblemDetails;

    validationProblemDetails?.Errors
      .Should()
      .ContainKey(expectedApiKeyKey);

    validationProblemDetails?.Errors[expectedApiKeyKey]
      .Should()
      .Contain(expectedApiKeyErrorMessage);
  }

  [Fact]
  public async Task AddGraph_WhenCalledWithNameAndApiKeyButUserNotFound_ItShouldReturn404StatusCodeWithProblemDetails()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, user.Id)
      ])));

    var dto = new AddGraphDto("Test Graph", "Test Api Key");

    _addGraphDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<AddGraphDto>(), default))
      .ReturnsAsync(new ValidationResult());

    var getUserResult = Result.Fail<User>(new UserDoesNotExistError(user.Id));

    _userServiceMock
      .Setup(s => s.GetUserByIdAsync(user.Id))
      .ReturnsAsync(getUserResult);

    var request = CreateAddGraphRequest(dto);

    var result = await GraphsController.AddGraph(request);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status404NotFound);

    var problemDetails = result.As<ProblemHttpResult>().ProblemDetails;

    problemDetails?.Extensions
      .Should()
      .ContainKey("Errors");

    problemDetails?.Extensions["Errors"]
      .Should()
      .BeEquivalentTo(getUserResult.Errors);
  }

  [Fact]
  public async Task AddGraph_WhenCalledWithNameAndApiKeyButAddGraphFailsBecauseNameAlreadyExists_ItShouldReturn409StatusCodeWithProblemDetails()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, user.Id)
      ])));

    var dto = new AddGraphDto("Test Graph", "Test Api Key");

    _addGraphDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<AddGraphDto>(), default))
      .ReturnsAsync(new ValidationResult());

    _userServiceMock
      .Setup(s => s.GetUserByIdAsync(user.Id))
      .ReturnsAsync(Result.Ok(user));

    var addGraphResult = Result.Fail<Graph>(new GraphAlreadyExistsError(dto.Name));

    _graphServiceMock
      .Setup(s => s.AddGraphAsync(It.IsAny<Graph>()))
      .ReturnsAsync(addGraphResult);

    var request = CreateAddGraphRequest(dto);

    var result = await GraphsController.AddGraph(request);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status409Conflict);

    var problemDetails = result.As<ProblemHttpResult>().ProblemDetails;

    problemDetails?.Extensions
      .Should()
      .ContainKey("Errors");

    problemDetails?.Extensions["Errors"]
      .Should()
      .BeEquivalentTo(addGraphResult.Errors);
  }

  [Fact]
  public async Task AddGraph_WhenCalledWithNameAndApiKey_ItShouldReturn201StatusCodeWithGraphDto()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, user.Id)
      ])));

    var apiKey = "Test Api Key";
    var encryptedApiKey = "Encrypted Test Api Key";

    var dto = new AddGraphDto("Test Graph", apiKey);

    var validationResult = new ValidationResult();

    _addGraphDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<AddGraphDto>(), default))
      .ReturnsAsync(validationResult);

    var createdGraph = new Graph(dto, user) { Id = Guid.NewGuid().ToString() };

    _userServiceMock
      .Setup(s => s.GetUserByIdAsync(user.Id))
      .ReturnsAsync(Result.Ok(user));

    _encryptionServiceMock
      .Setup(s => s.EncryptForUserAsync(apiKey, user))
      .ReturnsAsync(encryptedApiKey);

    _graphServiceMock
      .Setup(s => s.AddGraphAsync(It.IsAny<Graph>()))
      .ReturnsAsync(Result.Ok(createdGraph));

    var request = CreateAddGraphRequest(dto);

    var result = await GraphsController.AddGraph(request);

    result.Should()
      .BeOfType<Created<AddGraphResponse>>();

    result.As<Created<AddGraphResponse>>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status201Created);

    result.As<Created<AddGraphResponse>>()
      .Value
      .Should()
      .BeEquivalentTo(new AddGraphResponse(createdGraph.Id));

    _graphServiceMock
      .Verify(
        s =>
          s.AddGraphAsync(
            It.Is<Graph>(
              g =>
                g.Name == dto.Name &&
                g.ApiKey == encryptedApiKey
            )
          ),
        Times.Once
      );

    _graphQueueMock
      .Verify(
        q => q.EnqueueAsync(It.IsAny<GraphQueueItem>()),
        Times.Once
      );
  }

  private GetGraphsRequest CreateGetGraphsRequest(int pageNumber, int pageSize) => new(
    _contextMock.Object,
    _graphServiceMock.Object,
    pageNumber,
    pageSize
  );

  [Fact]
  public async Task GetGraphs_WhenCalledByUnauthenticatedUser_ItShouldReturn401StatusCode()
  {
    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal());

    var request = CreateGetGraphsRequest(1, 10);

    var result = await GraphsController.GetGraphs(request);

    result.Should()
      .BeOfType<UnauthorizedHttpResult>();

    result.As<UnauthorizedHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status401Unauthorized);
  }

  [Fact]
  public async Task GetGraphs_WhenCalled_ItShouldReturnPageOfGraphs()
  {
    var pageNumber = 1;
    var pageSize = 10;
    var userId = "userId";

    var graphs = FakeDataFactory.Graph
      .Generate(10)
      .Select(g =>
      {
        g.UserId = userId;
        return g;
      })
      .ToList();

    var page = new Page<Graph>(pageNumber, pageSize, 10, graphs);
    var getGraphsResponse = new GetGraphsResponse(page);

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, userId)
      ])));

    _graphServiceMock
      .Setup(s => s.GetGraphsAsync(pageNumber, pageSize, userId))
      .ReturnsAsync(page);

    var request = CreateGetGraphsRequest(pageNumber, pageSize);

    var result = await GraphsController.GetGraphs(request);

    result.Should()
      .BeOfType<Ok<GetGraphsResponse>>();

    result.As<Ok<GetGraphsResponse>>()
      .Value
      .Should()
      .BeEquivalentTo(getGraphsResponse);

    _graphServiceMock
      .Verify(
        s =>
          s.GetGraphsAsync(
            It.Is<int>(p => p == pageNumber),
            It.Is<int>(s => s == pageSize),
            It.Is<string>(u => u == userId)
          ),
        Times.Once
      );
  }

  private GetGraphRequest CreateGetGraphRequest(string id) => new(
    _contextMock.Object,
    id,
    _graphServiceMock.Object
  );

  [Fact]
  public async Task GetGraph_WhenCalledByUnauthenticatedUser_ItShouldReturn401StatusCode()
  {
    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal());

    var request = CreateGetGraphRequest("id");

    var result = await GraphsController.GetGraph(request);

    result.Should()
      .BeOfType<UnauthorizedHttpResult>();

    result.As<UnauthorizedHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status401Unauthorized);
  }

  [Fact]
  public async Task GetGraph_WhenCalledWithInvalidId_ItShouldReturn400StatusCode()
  {
    var id = "id";
    var userId = "userId";

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, userId)
      ])));

    var getGraphResult = Result.Fail<Graph>(new GraphNotFoundError(id));

    _graphServiceMock
      .Setup(s => s.GetGraphAsync(id, userId))
      .ReturnsAsync(getGraphResult);

    var request = CreateGetGraphRequest(id);

    var result = await GraphsController.GetGraph(request);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status400BadRequest);

    var validationProblemDetails = result.As<ProblemHttpResult>()
      .ProblemDetails as HttpValidationProblemDetails;

    validationProblemDetails!.Errors
      .Should()
      .ContainKey("Id");

    validationProblemDetails!.Errors["Id"]
      .Should()
      .Contain("Invalid graph id");
  }

  [Fact]
  public async Task GetGraph_WhenCalledAndGraphDoesNotExist_ItShouldReturn404StatusCode()
  {
    var graph = FakeDataFactory.Graph.Generate();

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, graph.UserId)
      ])));

    var getGraphResult = Result.Fail<Graph>(new GraphNotFoundError(graph.Id));

    _graphServiceMock
      .Setup(s => s.GetGraphAsync(graph.Id, graph.UserId))
      .ReturnsAsync(getGraphResult);

    var request = CreateGetGraphRequest(graph.Id);

    var result = await GraphsController.GetGraph(request);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status404NotFound);

    var problemDetails = result.As<ProblemHttpResult>().ProblemDetails;

    problemDetails.Title
      .Should()
      .Be("Failed to get graph");

    problemDetails.Detail
      .Should()
      .Be("Unable to retrieve graph. See errors for details.");

    problemDetails.Extensions
      .Should()
      .ContainKey("Errors");

    problemDetails.Extensions["Errors"]
      .Should()
      .BeEquivalentTo(getGraphResult.Errors);
  }

  [Fact]
  public async Task GetGraph_WhenCalledAndGraphExists_ItShouldReturnGraph()
  {
    var graph = FakeDataFactory.Graph.Generate();

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, graph.UserId)
      ])));

    var getGraphResult = Result.Ok(graph);

    _graphServiceMock
      .Setup(s => s.GetGraphAsync(graph.Id, graph.UserId))
      .ReturnsAsync(getGraphResult);

    var request = CreateGetGraphRequest(graph.Id);

    var result = await GraphsController.GetGraph(request);

    result.Should()
      .BeOfType<Ok<GraphDto>>();

    result.As<Ok<GraphDto>>()
      .Value
      .Should()
      .BeEquivalentTo(new GraphDto(graph));
  }

  private GetGraphKeyRequest CreateGetGraphKeyRequest(string id) => new(
    _contextMock.Object,
    id,
    _graphServiceMock.Object,
    _userServiceMock.Object,
    _encryptionServiceMock.Object
  );

  [Fact]
  public async Task GetGraphKey_WhenCalledByUnauthenticatedUser_ItShouldReturn401StatusCode()
  {
    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal());

    var request = CreateGetGraphKeyRequest("id");

    var result = await GraphsController.GetGraphKey(request);

    result.Should()
      .BeOfType<UnauthorizedHttpResult>();

    result.As<UnauthorizedHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status401Unauthorized);
  }

  private DeleteGraphRequest CreateDeleteGraphRequest(string id) => new(
    _contextMock.Object,
    id,
    _userServiceMock.Object,
    _graphServiceMock.Object
  );

  [Fact]
  public async Task DeleteGraph_WhenCalledByUnauthenticatedUser_ItShouldReturn401StatusCode()
  {
    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal());

    var request = CreateDeleteGraphRequest("id");

    var result = await GraphsController.DeleteGraph(request);

    result.Should()
      .BeOfType<UnauthorizedHttpResult>();

    result.As<UnauthorizedHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status401Unauthorized);
  }

  private UpdateGraphRequest CreateUpdateGraphRequest(string id, GraphDto dto) => new(
    _contextMock.Object,
    id,
    dto,
    _graphDtoValidatorMock.Object,
    _graphServiceMock.Object,
    _userServiceMock.Object
  );

  [Fact]
  public async Task UpdateGraph_WhenCalledByUnauthenticatedUser_ItShouldReturn401StatusCode()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var graphDto = new GraphDto(graph);

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal());

    var request = CreateUpdateGraphRequest("id", graphDto);

    var result = await GraphsController.UpdateGraph(request);

    result.Should()
      .BeOfType<UnauthorizedHttpResult>();

    result.As<UnauthorizedHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status401Unauthorized);
  }

  [Fact]
  public async Task UpdateGraph_WhenCalledAndUpdateFails_ItShouldReturn500StatusCodeWithProblemDetails()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var graph = FakeDataFactory.Graph.Generate();
    graph.UserId = user.Id;
    var graphDto = new GraphDto(graph);

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, user.Id)
      ])));

    _graphDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<GraphDto>(), default))
      .ReturnsAsync(new ValidationResult());

    _userServiceMock
      .Setup(s => s.GetUserByIdAsync(user.Id))
      .ReturnsAsync(Result.Ok(user));

    _graphServiceMock
      .Setup(s => s.GetGraphAsync(graph.Id, user.Id))
      .ReturnsAsync(Result.Ok(graph));

    _graphServiceMock
      .Setup(s => s.UpdateGraphAsync(It.IsAny<Graph>()))
      .ReturnsAsync(Result.Fail(new Error("Update failed")));

    var request = CreateUpdateGraphRequest(graph.Id, graphDto);

    var result = await GraphsController.UpdateGraph(request);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status500InternalServerError);

    var problemDetails = result.As<ProblemHttpResult>().ProblemDetails;

    problemDetails.Title
      .Should()
      .Be("Failed to update graph");
  }

  [Fact]
  public async Task UpdateGraph_WhenCalledAndGettingUpdatedGraphFails_ItShouldReturn500StatusCodeWithProblemDetails()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var graph = FakeDataFactory.Graph.Generate();
    graph.UserId = user.Id;
    var graphDto = new GraphDto(graph);

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, user.Id)
      ])));

    _graphDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<GraphDto>(), default))
      .ReturnsAsync(new ValidationResult());

    _userServiceMock
      .Setup(s => s.GetUserByIdAsync(user.Id))
      .ReturnsAsync(Result.Ok(user));

    _graphServiceMock
      .SetupSequence(s => s.GetGraphAsync(graph.Id, user.Id))
      .ReturnsAsync(Result.Ok(graph))
      .ReturnsAsync(Result.Fail(new Error("Get failed")));

    _graphServiceMock
      .Setup(s => s.UpdateGraphAsync(It.IsAny<Graph>()))
      .ReturnsAsync(Result.Ok(graph));

    var request = CreateUpdateGraphRequest(graph.Id, graphDto);

    var result = await GraphsController.UpdateGraph(request);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status500InternalServerError);

    var problemDetails = result.As<ProblemHttpResult>().ProblemDetails;

    problemDetails.Title
      .Should()
      .Be("Failed to update graph");
  }

  private UpdateGraphKeyRequest CreateUpdateGraphKeyRequest(string id, UpdateGraphKeyDto dto) => new(
    _contextMock.Object,
    id,
    dto,
    _updateGraphKeyDtoValidatorMock.Object,
    _graphServiceMock.Object,
    _userServiceMock.Object,
    _encryptionServiceMock.Object
  );

  [Fact]
  public async Task UpdateGraphKey_WhenCalledByUnauthenticatedUser_ItShouldReturn401StatusCode()
  {
    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal());

    var request = CreateUpdateGraphKeyRequest("id", new UpdateGraphKeyDto("key"));

    var result = await GraphsController.UpdateGraphKey(request);

    result.Should()
      .BeOfType<UnauthorizedHttpResult>();

    result.As<UnauthorizedHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status401Unauthorized);
  }

  [Fact]
  public async Task UpdateGraphKey_WhenUpdateFails_ItShouldReturn500StatusCodeWithProblemDetails()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var graph = FakeDataFactory.Graph.Generate();
    graph.UserId = user.Id;

    var updateGraphKeyDto = new UpdateGraphKeyDto("key");

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, user.Id)
      ])));

    _updateGraphKeyDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<UpdateGraphKeyDto>(), default))
      .ReturnsAsync(new ValidationResult());

    _userServiceMock
      .Setup(s => s.GetUserByIdAsync(user.Id))
      .ReturnsAsync(Result.Ok(user));

    _graphServiceMock
      .Setup(s => s.GetGraphAsync(graph.Id, user.Id))
      .ReturnsAsync(Result.Ok(graph));

    _graphServiceMock
      .Setup(s => s.UpdateGraphAsync(It.IsAny<Graph>()))
      .ReturnsAsync(Result.Fail(new Error("Update failed")));

    var request = CreateUpdateGraphKeyRequest(graph.Id, updateGraphKeyDto);

    var result = await GraphsController.UpdateGraphKey(request);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status500InternalServerError);

    var problemDetails = result.As<ProblemHttpResult>().ProblemDetails;

    problemDetails.Title
      .Should()
      .Be("Failed to update graph key");
  }

  private RefreshGraphRequest CreateRefreshGraphRequest(string id) => new(
    _contextMock.Object,
    id,
    _graphServiceMock.Object,
    _graphQueueMock.Object,
    _userServiceMock.Object
  );

  [Fact]
  public async Task RefreshGraph_WhenCalledByUnauthenticatedUser_ItShouldReturn401StatusCode()
  {
    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal());

    var request = CreateRefreshGraphRequest("id");

    var result = await GraphsController.RefreshGraph(request);

    result.Should()
      .BeOfType<UnauthorizedHttpResult>();

    result.As<UnauthorizedHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status401Unauthorized);
  }

  [Fact]
  public async Task RefreshGraph_WhenRefreshFails_ItShouldReturn500StatusCodeWithProblemDetails()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var graph = FakeDataFactory.Graph.Generate();
    graph.UserId = user.Id;

    _contextMock
      .Setup(c => c.User)
      .Returns(new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, user.Id)
      ])));

    _userServiceMock
      .Setup(s => s.GetUserByIdAsync(user.Id))
      .ReturnsAsync(Result.Ok(user));

    _graphServiceMock
      .Setup(s => s.GetGraphAsync(graph.Id, user.Id))
      .ReturnsAsync(Result.Ok(graph));

    _graphServiceMock
      .Setup(s => s.UpdateGraphAsync(graph))
      .ReturnsAsync(Result.Fail(new Error("Refresh failed")));

    var request = CreateRefreshGraphRequest(graph.Id);

    var result = await GraphsController.RefreshGraph(request);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be(StatusCodes.Status500InternalServerError);

    var problemDetails = result.As<ProblemHttpResult>().ProblemDetails;

    problemDetails.Title
      .Should()
      .Be("Failed to refresh graph");

    _graphQueueMock
      .Verify(
        q => q.EnqueueAsync(It.IsAny<GraphQueueItem>()),
        Times.Never
      );
  }
}