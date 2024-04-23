using ValidationFailure = FluentValidation.Results.ValidationFailure;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Server.API.Tests.Unit;

public class GraphsControllerTests
{
  private readonly Mock<HttpContext> _contextMock = new();
  private readonly Mock<IValidator<AddGraphDto>> _addGraphDtoValidatorMock = new();
  private readonly Mock<IGraphService> _graphServiceMock = new();
  private readonly Mock<IUserService> _userServiceMock = new();
  private readonly Mock<IEncryptionService> _encryptionServiceMock = new();

  private AddGraphRequest CreateAddGraphRequest(AddGraphDto dto) => new(
    _contextMock.Object,
    dto,
    _addGraphDtoValidatorMock.Object,
    _graphServiceMock.Object,
    _userServiceMock.Object,
    _encryptionServiceMock.Object
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
      .Setup(s => s.AddGraph(It.IsAny<Graph>()))
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
      .Setup(s => s.AddGraph(It.IsAny<Graph>()))
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
          s.AddGraph(
            It.Is<Graph>(
              g =>
                g.Name == dto.Name &&
                g.ApiKey == encryptedApiKey
            )
          ),
        Times.Once
      );
  }
}