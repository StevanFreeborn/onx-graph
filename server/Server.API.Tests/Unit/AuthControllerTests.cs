using System.Net;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Server.API.Tests.Unit;

public class AuthControllerTests
{
  private readonly Mock<IUserService> _userServiceMock = new();
  private readonly Mock<IValidator<RegisterDto>> _validatorMock = new();

  [Fact]
  public async Task Register_WhenProvidedEmailIsInvalid_ReturnsValidationProblem()
  {
    var expectedEmailKey = "Email";
    var expectedErrorMessage = "Email must be a valid email address.";

    var dto = Mock.Of<RegisterDto>();
    var req = new RegisterRequest(
      dto,
      _validatorMock.Object,
      _userServiceMock.Object
    );

    var validationResult = new ValidationResult(
      new[]
      {
        new ValidationFailure(expectedEmailKey, expectedErrorMessage)
      }
    );

    _validatorMock
      .Setup(v => v.ValidateAsync(dto, default))
      .ReturnsAsync(validationResult);

    var result = await AuthController.Register(req);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be((int)HttpStatusCode.BadRequest);

    var validationProblemDetails = result.As<ProblemHttpResult>()
      .ProblemDetails as ValidationProblemDetails;

    validationProblemDetails?.Errors
      .Should()
      .ContainKey(expectedEmailKey);

    validationProblemDetails?.Errors[expectedEmailKey]
      .Should()
      .Contain(expectedErrorMessage);
  }

  [Fact]
  public async Task Register_WhenProvidedPasswordIsInvalid_ReturnsValidationProblem()
  {
    var expectedPasswordKey = "Password";
    var expectedErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number and one special character.";

    var dto = Mock.Of<RegisterDto>();
    var req = new RegisterRequest(
      dto,
      _validatorMock.Object,
      _userServiceMock.Object
    );

    var validationResult = new ValidationResult(
      new[]
      {
        new ValidationFailure(expectedPasswordKey, expectedErrorMessage)
      }
    );

    _validatorMock
      .Setup(v => v.ValidateAsync(dto, default))
      .ReturnsAsync(validationResult);

    var result = await AuthController.Register(req);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be((int)HttpStatusCode.BadRequest);

    var validationProblemDetails = result.As<ProblemHttpResult>()
      .ProblemDetails as ValidationProblemDetails;

    validationProblemDetails?.Errors
      .Should()
      .ContainKey(expectedPasswordKey);

    validationProblemDetails?.Errors[expectedPasswordKey]
      .Should()
      .Contain(expectedErrorMessage);
  }
}