using ValidationFailure = FluentValidation.Results.ValidationFailure;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Server.API.Tests.Unit;

public class GraphsControllerTests
{
  private readonly Mock<HttpContext> _context = new();
  private readonly Mock<IValidator<AddGraphDto>> _addGraphDtoValidator = new();

  private AddGraphRequest CreateAddGraphRequest(AddGraphDto dto) => new(
    _context.Object,
    dto,
    _addGraphDtoValidator.Object
  );

  [Fact]
  public async Task AddGraph_WhenCalledByUnauthenticatedUser_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    _context
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

    _context
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

    _addGraphDtoValidator
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

    _context
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

    _addGraphDtoValidator
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
}