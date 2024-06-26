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
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

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
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

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
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

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
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

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
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

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
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

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
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
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

  [Fact]
  public async Task GetGraphKey_WhenCalledByUnauthenticatedUser_ItShouldReturnUnauthorized()
  {
    var graph = FakeDataFactory.Graph.Generate();

    var response = await _client.GetAsync($"/graphs/{graph.Id}/key");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetGraphKey_WhenCalledWithInvalidGraphId_ItShouldReturnBadRequest()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.GetAsync("/graphs/invalid-graph-id/key");

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GetGraphKey_WhenCalledAndUserDoesNotExist_ItShouldReturnNotFound()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.GetAsync($"/graphs/{graph.Id}/key");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetGraphKey_WhenCalledAndGraphDoesNotExist_ItShouldReturnNotFound()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);

    var response = await _client.GetAsync($"/graphs/{graph.Id}/key");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetGraphKey_WhenCalledAndGraphExists_ItShouldReturnGraphKey()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var unencryptedApiKey = graph.ApiKey;

    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var encryptedGraphApiKey = await EncryptionService.EncryptForUserAsync(graph.ApiKey, user);
    graph.ApiKey = encryptedGraphApiKey;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);
    await Context.Graphs.InsertOneAsync(graph);

    var response = await _client.GetAsync($"/graphs/{graph.Id}/key");

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var responseBody = await response.Content.ReadFromJsonAsync<GetGraphKeyResponse>();

    responseBody!.Key.Should().Be(unencryptedApiKey);
  }

  [Fact]
  public async Task DeleteGraph_WhenCalledByUnauthenticatedUser_ItShouldReturnUnauthorized()
  {
    var response = await _client.DeleteAsync("/graphs/123");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task DeleteGraph_WhenCalledWithInvalidGraphId_ItShouldReturnBadRequest()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.DeleteAsync("/graphs/invalid-graph-id");

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task DeleteGraph_WhenCalledAndUserDoesNotExist_ItShouldReturnNotFound()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.DeleteAsync($"/graphs/{graph.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteGraph_WhenCalledAndGraphDoesNotExist_ItShouldReturnNotFound()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);

    var response = await _client.DeleteAsync($"/graphs/{graph.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteGraph_WhenCalledAndGraphExists_ItShouldDeleteGraph()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);
    await Context.Graphs.InsertOneAsync(graph);

    var response = await _client.DeleteAsync($"/graphs/{graph.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    var deletedGraph = await Context.Graphs
      .Find(g => g.Id == graph.Id)
      .SingleOrDefaultAsync();

    deletedGraph.Should().BeNull();
  }

  [Fact]
  public async Task UpdateGraph_WhenCalledByUnauthenticatedUser_ItShouldReturnUnauthorized()
  {
    var response = await _client.PutAsJsonAsync("/graphs/123", new { });

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UpdateGraph_WhenCalledWithInvalidGraphId_ItShouldReturnBadRequest()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.PutAsJsonAsync("/graphs/invalid-graph-id", new { });

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateGraph_WhenCalledAndNameIsEmpty_ItShouldReturnBadRequest()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);
    await Context.Graphs.InsertOneAsync(graph);

    var response = await _client.PutAsJsonAsync($"/graphs/{graph.Id}", new { name = string.Empty });

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var responseBody = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    responseBody!.Errors["Name"].Should().Contain("'Name' must not be empty.");
  }

  [Fact]
  public async Task UpdateGraph_WhenCalledAndGraphsUserNotFound_ItShouldReturnNotFound()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Graphs.InsertOneAsync(graph);

    var response = await _client.PutAsJsonAsync($"/graphs/{graph.Id}", new { name = "Updated Graph" });

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UpdateGraph_WhenCalledAndGraphDoesNotExist_ItShouldReturnNotFound()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);

    var response = await _client.PutAsJsonAsync($"/graphs/{graph.Id}", new { name = "Updated Graph" });

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UpdateGraph_WhenCalledAndGraphAlreadyExistsWithSameName_ItShouldReturnConflict()
  {
    var existingGraph = FakeDataFactory.Graph.Generate();
    var updatedGraph = FakeDataFactory.Graph.Generate();
    updatedGraph.Name = existingGraph.Name;

    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    existingGraph.UserId = user.Id;
    updatedGraph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);
    await Context.Graphs.InsertOneAsync(existingGraph);
    await Context.Graphs.InsertOneAsync(updatedGraph);

    var graphDto = new GraphDto(updatedGraph);
    var response = await _client.PutAsJsonAsync($"/graphs/{updatedGraph.Id}", graphDto);

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
  }

  [Fact]
  public async Task UpdateGraph_WhenCalledAndUpdateSucceeds_ItShouldReturnOkWithUpdatedGraph()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);
    await Context.Graphs.InsertOneAsync(graph);

    var updatedName = "Updated Graph";
    var layout = new Dictionary<string, Point>() { { "1", new(1, 1) } };
    graph.Name = updatedName;
    var graphDto = new GraphDto(graph) with { Layout = layout };
    var response = await _client.PutAsJsonAsync($"/graphs/{graphDto.Id}", graphDto);

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var responseBody = await response.Content.ReadFromJsonAsync<GraphDto>();

    responseBody!.Id.Should().Be(graph.Id);
    responseBody!.Name.Should().Be(graphDto.Name);
    responseBody!.UpdatedAt.Should().BeAfter(graphDto.UpdatedAt);
    responseBody!.Layout.Should().BeEquivalentTo(layout);
  }

  [Fact]
  public async Task UpdateGraphKey_WhenCalledByUnauthenticatedUser_ItShouldReturnUnauthorized()
  {
    var response = await _client.PatchAsJsonAsync("/graphs/123/key", new { });

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UpdateGraphKey_WhenCalledWithInvalidGraphId_ItShouldReturnBadRequest()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.PatchAsJsonAsync("/graphs/invalid-graph-id/key", new { });

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateGraphKey_WhenCalledWithEmptyApiKey_ItShouldReturnBadRequest()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.PatchAsJsonAsync($"/graphs/{graph.Id}/key", new { key = string.Empty });

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var responseBody = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    responseBody!.Errors["Key"].Should().Contain("'Key' must not be empty.");
  }

  [Fact]
  public async Task UpdateGraphKey_WhenCalledAndGraphsUserNotFound_ItShouldReturnNotFound()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.PatchAsJsonAsync($"/graphs/{graph.Id}/key", new { key = "Updated Key" });

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UpdateGraphKey_WhenCalledAndGraphDoesNotExist_ItShouldReturnNotFound()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);

    var response = await _client.PatchAsJsonAsync($"/graphs/{graph.Id}/key", new { key = "Updated Key" });

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UpdateGraphKey_WhenCalledAndUpdateSucceeds_ItShouldReturnNoContent()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);
    await Context.Graphs.InsertOneAsync(graph);

    var updatedApiKey = "Updated Key";
    var response = await _client.PatchAsJsonAsync($"/graphs/{graph.Id}/key", new { key = updatedApiKey });

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    var updatedGraph = await Context.Graphs
      .Find(g => g.Id == graph.Id)
      .SingleOrDefaultAsync();

    updatedGraph.Should().NotBeNull();

    var decryptedApiKey = await EncryptionService.DecryptForUserAsync(updatedGraph.ApiKey, user);

    decryptedApiKey.Should().Be(updatedApiKey);
  }

  [Fact]
  public async Task RefreshGraph_WhenCalledByUnauthenticatedUser_ItShouldReturnUnauthorized()
  {
    var response = await _client.PatchAsync("/graphs/123/refresh", null);

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task RefreshGraph_WhenCalledWithInvalidGraphId_ItShouldReturnBadRequest()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.PatchAsync("/graphs/invalid-graph-id/refresh", null);

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task RefreshGraph_WhenCalledAndGraphsUserNotFound_ItShouldReturnNotFound()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    var response = await _client.PatchAsync($"/graphs/{graph.Id}/refresh", null);

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task RefreshGraph_WhenCalledAndGraphDoesNotExist_ItShouldReturnNotFound()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);

    var response = await _client.PatchAsync($"/graphs/{graph.Id}/refresh", null);

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task RefreshGraph_WhenCalledAndGraphExists_ItShouldReturnNoContent()
  {
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var encryptionKey = EncryptionService.GenerateKey();
    var encryptedUserEncryptionKey = await EncryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedUserEncryptionKey;
    graph.UserId = user.Id;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    _client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);

    await Context.Users.InsertOneAsync(user);
    await Context.Graphs.InsertOneAsync(graph);

    var response = await _client.PatchAsync($"/graphs/{graph.Id}/refresh", null);

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    var updatedGraph = await Context.Graphs
      .Find(g => g.Id == graph.Id)
      .SingleOrDefaultAsync();

    updatedGraph.Should().NotBeNull();
    updatedGraph.UpdatedAt.Should().BeAfter(graph.UpdatedAt);
    updatedGraph.Status.Should().Be(GraphStatus.Building);
  }
}