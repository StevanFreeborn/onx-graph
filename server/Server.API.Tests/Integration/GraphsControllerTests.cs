namespace Server.API.Tests.Integration;

public class GraphsControllerTests(TestServerFactory serverFactory) : IntegrationTest(serverFactory), IDisposable
{
  public void Dispose()
  {
    GC.SuppressFinalize(this);
  }

  [Fact]
  public async Task AddGraph_WhenCalledByUnauthenticatedUser_ItShouldReturnUnauthorized()
  {
    var response = await _client.PostAsJsonAsync("/graphs/add", new { });

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task AddGraph_WhenCalledWithoutName_ItShouldReturnBadRequest()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.PostAsJsonAsync("/graphs/add", new { apiKey = "API Key" });

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var responseBody = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    responseBody!.Errors["Name"].Should().Contain("'Name' must not be empty.");
  }

  [Fact]
  public async Task AddGraph_WhenCalledWithoutApiKey_ItShouldReturnBadRequest()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.PostAsJsonAsync("/graphs/add", new { name = "Test Graph" });

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var responseBody = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    responseBody!.Errors["ApiKey"].Should().Contain("'Api Key' must not be empty.");
  }

  [Fact]
  public async Task AddGraph_WhenCalledAndUserDoesNotExist_ItShouldReturnNotFound()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.PostAsJsonAsync("/graphs/add", new { name = "Test Graph", apiKey = "API Key" });

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task AddGraph_WhenCalledAndGraphWithSameNameAlreadyExists_ItShouldReturnConflict()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var existingGraph = FakeDataFactory.Graph.Generate();

    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedApiKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedApiKey;

    existingGraph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);
    await Context.Graphs.InsertOneAsync(existingGraph);

    var requestBody = new { name = existingGraph.Name, apiKey = "API Key" };

    var response = await _client.PostAsJsonAsync("/graphs/add", requestBody);

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
  }

  [Fact]
  public async Task AddGraph_WhenCalledAndUserExists_ItShouldReturnCreatedGraph()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedApiKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedApiKey;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);

    var requestBody = new { name = "Test Graph", apiKey = "API Key" };

    var response = await _client.PostAsJsonAsync("/graphs/add", requestBody);

    response.StatusCode.Should().Be(HttpStatusCode.Created);

    var responseBody = await response.Content.ReadFromJsonAsync<AddGraphResponse>();

    responseBody!.Id.Should().NotBeNullOrEmpty();

    var createdGraph = await Context.Graphs
      .Find(g => g.Id == responseBody.Id)
      .SingleOrDefaultAsync();

    createdGraph.Should().NotBeNull();
    createdGraph.ApiKey.Should().NotBe(requestBody.apiKey);

    var decryptedApiKey = await EncryptionService.DecryptForUserAsync(createdGraph.ApiKey, user);

    decryptedApiKey.Should().Be(requestBody.apiKey);
  }
}