namespace Server.API.Tests.Unit;

public class AuthControllerTests
{
  private readonly Mock<IUserService> _userServiceMock = new();
  private readonly Mock<IValidator<RegisterDto>> _validatorMock = new();

  [Fact]
  public async Task Register_WhenRegistrationFails_ItShouldReturnProblemDetailWith409StatusCode()
  {
    var dto = new RegisterDto("test@test.com", "@Password1234");
    var validationResult = new ValidationResult();

    _validatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<RegisterDto>(), default))
      .ReturnsAsync(validationResult);

    var registrationResult = Result.Fail(new UserAlreadyExistError(dto.Email));

    _userServiceMock
      .Setup(u => u.RegisterUserAsync(It.IsAny<User>()))
      .ReturnsAsync(registrationResult);

    var req = new RegisterRequest(
      dto,
      _validatorMock.Object,
      _userServiceMock.Object
    );

    var result = await AuthController.Register(req);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be((int)HttpStatusCode.Conflict);

    result.As<ProblemHttpResult>()
      .ProblemDetails
      .Title
      .Should()
      .Be("Registration failed");

    result.As<ProblemHttpResult>()
      .ProblemDetails
      .Detail
      .Should()
      .Be("Unable to register user. See errors for details.");

    result.As<ProblemHttpResult>()
      .ProblemDetails
      .Extensions
      .Should()
      .ContainKey("Errors");
  }


  [Fact]
  public async Task Register_WhenProvidedEmailIsInvalid_ItShouldReturnValidationProblemWith400StatusCode()
  {
    var expectedEmailKey = "Email";
    var expectedErrorMessage = "Email must be a valid email address.";

    var dto = Mock.Of<RegisterDto>();
    var validationResult = new ValidationResult(
      new[]
      {
        new ValidationFailure(expectedEmailKey, expectedErrorMessage)
      }
    );

    _validatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<RegisterDto>(), default))
      .ReturnsAsync(validationResult);

    var req = new RegisterRequest(
      dto,
      _validatorMock.Object,
      _userServiceMock.Object
    );

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
  public async Task Register_WhenProvidedPasswordIsInvalid_ItShouldReturnValidationProblemWith400StatusCode()
  {
    var expectedPasswordKey = "Password";
    var expectedErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number and one special character.";

    var dto = Mock.Of<RegisterDto>();
    var validationResult = new ValidationResult(
      new[]
      {
        new ValidationFailure(expectedPasswordKey, expectedErrorMessage)
      }
    );

    _validatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<RegisterDto>(), default))
      .ReturnsAsync(validationResult);

    var req = new RegisterRequest(
      dto,
      _validatorMock.Object,
      _userServiceMock.Object
    );

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