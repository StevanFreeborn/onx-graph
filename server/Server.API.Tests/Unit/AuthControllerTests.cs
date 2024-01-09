using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Server.API.Tests.Unit;

public class AuthControllerTests
{
  private readonly Mock<HttpContext> _httpContextMock = new();
  private readonly Mock<IUserService> _userServiceMock = new();
  private readonly Mock<IValidator<RegisterDto>> _registerDtoValidatorMock = new();
  private readonly Mock<IValidator<LoginDto>> _loginDtoValidatorMock = new();

  [Fact]
  public async Task Register_WhenRegistrationFails_ItShouldReturnProblemDetailWith409StatusCode()
  {
    var dto = new RegisterDto("test@test.com", "@Password1234");
    var validationResult = new ValidationResult();

    _registerDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<RegisterDto>(), default))
      .ReturnsAsync(validationResult);

    var registrationResult = Result.Fail(new UserAlreadyExistError(dto.Email));

    _userServiceMock
      .Setup(u => u.RegisterUserAsync(It.IsAny<User>()))
      .ReturnsAsync(registrationResult);

    var req = new RegisterRequest(
      dto,
      _registerDtoValidatorMock.Object,
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
  public async Task Register_WhenProvidedEmailIsInvalid_ItShouldReturnValidationProblemDetailWith400StatusCode()
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

    _registerDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<RegisterDto>(), default))
      .ReturnsAsync(validationResult);

    var req = new RegisterRequest(
      dto,
      _registerDtoValidatorMock.Object,
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
  public async Task Register_WhenProvidedPasswordIsInvalid_ItShouldReturnValidationProblemDetailWith400StatusCode()
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

    _registerDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<RegisterDto>(), default))
      .ReturnsAsync(validationResult);

    var req = new RegisterRequest(
      dto,
      _registerDtoValidatorMock.Object,
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

  [Fact]
  public async Task Register_WhenRegistrationSucceeds_ItShouldReturnARegisterUserResponseWith201StatusCode()
  {
    var dto = new RegisterDto("test@test.com", "@Password1234");
    var validationResult = new ValidationResult();

    _registerDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<RegisterDto>(), default))
      .ReturnsAsync(validationResult);

    var registrationResult = Result.Ok("test");

    _userServiceMock
      .Setup(u => u.RegisterUserAsync(It.IsAny<User>()))
      .ReturnsAsync(registrationResult);

    var req = new RegisterRequest(
      dto,
      _registerDtoValidatorMock.Object,
      _userServiceMock.Object
    );

    var result = await AuthController.Register(req);

    result.Should()
      .BeOfType<Created<RegisterUserResponse>>();

    var createdResponse = result.As<Created<RegisterUserResponse>>();

    createdResponse.StatusCode
      .Should()
      .Be((int)HttpStatusCode.Created);

    createdResponse.Value
      .Should()
      .BeOfType<RegisterUserResponse>();

    createdResponse.Value?.Id
      .Should()
      .Be(registrationResult.Value);
  }

  [Fact]
  public async Task Login_WhenNoEmailIsProvided_ItShouldReturnAValidationProblemDetailWith400StatusCode()
  {
    var expectedEmailKey = "Email";
    var expectedErrorMessage = "'Email' must not be empty.";

    var dto = Mock.Of<LoginDto>();
    var validationResult = new ValidationResult(
      new[]
      {
        new ValidationFailure(expectedEmailKey, expectedErrorMessage)
      }
    );

    _loginDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<LoginDto>(), default))
      .ReturnsAsync(validationResult);

    var req = new LoginRequest(
      _httpContextMock.Object,
      dto,
      _loginDtoValidatorMock.Object,
      _userServiceMock.Object
    );

    var result = await AuthController.Login(req);

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
  public async Task Login_WhenNoPasswordIsProvided_ItShouldReturnAValidationProblemDetailWith400StatusCode()
  {
    var expectedPasswordKey = "Password";
    var expectedErrorMessage = "'Password' must not be empty.";

    var dto = Mock.Of<LoginDto>();
    var validationResult = new ValidationResult(
      new[]
      {
        new ValidationFailure(expectedPasswordKey, expectedErrorMessage)
      }
    );

    _loginDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<LoginDto>(), default))
      .ReturnsAsync(validationResult);

    var req = new LoginRequest(
      _httpContextMock.Object,
      dto,
      _loginDtoValidatorMock.Object,
      _userServiceMock.Object
    );

    var result = await AuthController.Login(req);

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

  [Fact]
  public async Task Login_WhenProvidedEmailIsInvalid_ItShouldReturnAValidationProblemDetailWith400StatusCode()
  {
    var expectedEmailKey = "Email";
    var expectedErrorMessage = "Email must be a valid email address.";

    var dto = Mock.Of<LoginDto>();
    var validationResult = new ValidationResult(
      new[]
      {
        new ValidationFailure(expectedEmailKey, expectedErrorMessage)
      }
    );

    _loginDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<LoginDto>(), default))
      .ReturnsAsync(validationResult);

    var req = new LoginRequest(
      _httpContextMock.Object,
      dto,
      _loginDtoValidatorMock.Object,
      _userServiceMock.Object
    );

    var result = await AuthController.Login(req);

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
  public async Task Login_WhenLoginFails_ItShouldReturnAProblemDetailWith401StatusCode()
  {
    var dto = new LoginDto("test", "@Password1234");
    var validationResult = new ValidationResult();

    _loginDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<LoginDto>(), default))
      .ReturnsAsync(validationResult);

    var loginResult = Result.Fail(new InvalidLoginError());

    _userServiceMock
      .Setup(u => u.LoginUserAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(loginResult);

    var req = new LoginRequest(
      _httpContextMock.Object,
      dto,
      _loginDtoValidatorMock.Object,
      _userServiceMock.Object
    );

    var result = await AuthController.Login(req);

    result.Should()
      .BeOfType<ProblemHttpResult>();

    result.As<ProblemHttpResult>()
      .StatusCode
      .Should()
      .Be((int)HttpStatusCode.Unauthorized);

    result.As<ProblemHttpResult>()
      .ProblemDetails
      .Title
      .Should()
      .Be("Login failed");
  }

  [Fact]
  public async Task Login_WhenLoginSucceeds_ItShouldReturnALoginUserResponseWith200StatusCode()
  {
    var dto = new LoginDto("test@test.com", "@Password1234");
    var validationResult = new ValidationResult();

    _loginDtoValidatorMock
      .Setup(v => v.ValidateAsync(It.IsAny<LoginDto>(), default))
      .ReturnsAsync(validationResult);

    var loginResult = Result.Ok((AccessToken: "accessToken", RefreshToken: new RefreshToken()));

    _userServiceMock
      .Setup(u => u.LoginUserAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(loginResult);

    var responseMock = new Mock<HttpResponse>();

    responseMock
      .Setup(r => r.Cookies)
      .Returns(new Mock<IResponseCookies>().Object);

    _httpContextMock
      .Setup(c => c.Response)
      .Returns(responseMock.Object);

    var req = new LoginRequest(
      _httpContextMock.Object,
      dto,
      _loginDtoValidatorMock.Object,
      _userServiceMock.Object
    );

    var result = await AuthController.Login(req);

    result.Should()
      .BeOfType<Ok<LoginUserResponse>>();

    var okResult = result.As<Ok<LoginUserResponse>>();

    okResult.StatusCode
      .Should()
      .Be((int)HttpStatusCode.OK);

    okResult.Value
      .Should()
      .BeOfType<LoginUserResponse>();

    var response = okResult.Value?.AccessToken
      .Should()
      .Be(loginResult.Value.AccessToken);
  }
}