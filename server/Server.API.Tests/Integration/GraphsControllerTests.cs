namespace Server.API.Tests.Integration;

public class GraphsControllerTests(TestServerFactory serverFactory) : IntegrationTest(serverFactory)
{
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
    var encryptedKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedKey;

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

  [Fact]
  public async Task GetGraphs_WhenCalledByUnauthenticatedUser_ItShouldReturnUnauthorized()
  {
    var response = await _client.GetAsync("/graphs");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetGraphs_WhenCalledAndUserExists_ItShouldReturnGraphs()
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

    var graphs = FakeDataFactory.Graph
      .Generate(5)
      .Select(g =>
      {
        g.UserId = user.Id;
        return g;
      })
      .ToList();

    await Context.Graphs.InsertManyAsync(graphs);

    var response = await _client.GetAsync("/graphs");

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var responseBody = await response.Content.ReadFromJsonAsync<GetGraphsResponse>();

    responseBody!.Data.Should().HaveCount(5);
    responseBody!.PageNumber.Should().Be(1);
    responseBody!.PageCount.Should().Be(5);
    responseBody!.TotalPages.Should().Be(1);
    responseBody!.TotalCount.Should().Be(5);
  }

  [Fact]
  public async Task GetGraphs_WhenCalledWithPageParameters_ItShouldReturnGraphs()
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

    var graphs = FakeDataFactory.Graph
      .Generate(10)
      .Select(g =>
      {
        g.UserId = user.Id;
        return g;
      })
      .ToList();

    await Context.Graphs.InsertManyAsync(graphs);

    var response = await _client.GetAsync("/graphs?pageNumber=2&pageSize=5");

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var responseBody = await response.Content.ReadFromJsonAsync<GetGraphsResponse>();

    responseBody!.Data.Should().HaveCount(5);
    responseBody!.PageNumber.Should().Be(2);
    responseBody!.PageCount.Should().Be(5);
    responseBody!.TotalPages.Should().Be(2);
    responseBody!.TotalCount.Should().Be(10);
  }

  [Fact]
  public async Task GetGraph_WhenCalledByUnauthenticatedUser_ItShouldReturnUnauthorized()
  {
    var response = await _client.GetAsync("/graphs/123");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetGraph_WhenCalledWithInvalidGraphId_ItShouldReturnBadRequest()
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

    var response = await _client.GetAsync("/graphs/invalid-graph-id");

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GetGraph_WhenCalledAndGraphDoesNotExist_ItShouldReturnNotFound()
  {
    var graph = FakeDataFactory.Graph.Generate();
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

    var response = await _client.GetAsync($"/graphs/{graph.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetGraph_WhenCalledAndGraphExists_ItShouldReturnGraph()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedApiKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedApiKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);
    await Context.Graphs.InsertOneAsync(graph);

    var response = await _client.GetAsync($"/graphs/{graph.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var responseBody = await response.Content.ReadFromJsonAsync<GraphDto>();

    responseBody!.Id.Should().Be(graph.Id);
    responseBody!.Name.Should().Be(graph.Name);
  }
}