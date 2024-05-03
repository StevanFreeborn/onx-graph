namespace Server.API.Tests.Unit;

public class GraphProcessorTests
{
  private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock = new();
  private readonly Mock<IHubContext<GraphsHub, IGraphsClient>> _hubContextMock = new();
  private readonly Mock<ILogger<GraphProcessor>> _loggerMock = new();
  private readonly Mock<IEncryptionService> _encryptionServiceMock = new();
  private readonly Mock<IOnspringClient> _onspringClientMock = new();
  private readonly Mock<IOnspringClientFactory> _onspringClientFactoryMock = new();
  private readonly Mock<IServiceScope> _serviceScopeMock = new();
  private readonly Mock<IServiceProvider> _serviceProviderMock = new();
  private readonly Mock<IGraphRepository> _graphRepositoryMock = new();
  private readonly Mock<IUserRepository> _userRepositoryMock = new();
  private readonly GraphProcessor _graphProcessor;

  public GraphProcessorTests()
  {
    _serviceScopeFactoryMock
      .Setup(f => f.CreateScope())
      .Returns(_serviceScopeMock.Object);

    _serviceScopeMock
      .Setup(s => s.ServiceProvider)
      .Returns(_serviceProviderMock.Object);

    _serviceProviderMock
      .Setup(p => p.GetService(typeof(IGraphRepository)))
      .Returns(_graphRepositoryMock.Object);

    _serviceProviderMock
      .Setup(p => p.GetService(typeof(IUserRepository)))
      .Returns(_userRepositoryMock.Object);

    _onspringClientFactoryMock
      .Setup(f => f.CreateClient(It.IsAny<string>()))
      .Returns(_onspringClientMock.Object);

    _graphProcessor = new GraphProcessor(
      _serviceScopeFactoryMock.Object,
      _hubContextMock.Object,
      _loggerMock.Object,
      _encryptionServiceMock.Object,
      _onspringClientFactoryMock.Object
    );
  }

  [Fact]
  public async Task ProcessAsync_WhenGraphNotFound_ItShouldLogWarning()
  {
    var item = new GraphQueueItem();

    _graphRepositoryMock
      .Setup(r => r.GetGraphAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(null as Graph);

    await _graphProcessor.ProcessAsync(item);

    _loggerMock.Verify(x => x.Log(
        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task ProcessAsync_WhenUserNotFound_ItShouldLogWarning()
  {
    var item = FakeDataFactory.GraphQueueItem.Generate();
    var graph = FakeDataFactory.Graph.Generate();

    _graphRepositoryMock
      .Setup(r => r.GetGraphAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(graph);

    _userRepositoryMock
      .Setup(r => r.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(null as User);

    await _graphProcessor.ProcessAsync(item);

    _loggerMock.Verify(x => x.Log(
        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task ProcessAsync_WhenConnectionToOnspringFails_ItShouldLogWarningUpdateGraphAndLogError()
  {
    var item = FakeDataFactory.GraphQueueItem.Generate();
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _graphRepositoryMock
      .Setup(r => r.GetGraphAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(graph);

    _userRepositoryMock
      .Setup(r => r.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    _onspringClientMock
      .Setup(c => c.CanConnectAsync())
      .ReturnsAsync(false);

    _hubContextMock
      .Setup(h => h.Clients.Group(It.IsAny<string>()))
      .Returns(Mock.Of<IGraphsClient>());

    await _graphProcessor.ProcessAsync(item);

    _loggerMock.Verify(x => x.Log(
        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
      ),
      Times.Once
    );

    _graphRepositoryMock
      .Verify(
        x => x.UpdateGraphAsync(It.Is<Graph>(g => g.Status == GraphStatus.NotBuilt)),
        Times.Once
      );

    _loggerMock.Verify(x => x.Log(
        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task ProcessAsync_WhenGraphHasNoApps_ItShouldLogWarningUpdateGraphAndLogError()
  {
    var item = FakeDataFactory.GraphQueueItem.Generate();
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _graphRepositoryMock
      .Setup(r => r.GetGraphAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(graph);

    _userRepositoryMock
      .Setup(r => r.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    _onspringClientMock
      .Setup(c => c.CanConnectAsync())
      .ReturnsAsync(true);

    _onspringClientMock
      .Setup(c => c.GetAppsAsync(It.IsAny<PagingRequest>()))
      .ReturnsAsync(new ApiResponse<GetPagedAppsResponse>()
      {
        StatusCode = HttpStatusCode.OK,
        Value = new()
        {
          Items = [],
          PageNumber = 1,
          TotalPages = 1
        }
      });

    _hubContextMock
      .Setup(h => h.Clients.Group(It.IsAny<string>()))
      .Returns(Mock.Of<IGraphsClient>());

    await _graphProcessor.ProcessAsync(item);

    _loggerMock.Verify(x => x.Log(
        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
      ),
      Times.Once
    );

    _graphRepositoryMock
      .Verify(
        x => x.UpdateGraphAsync(It.Is<Graph>(g => g.Status == GraphStatus.NotBuilt)),
        Times.Once
      );

    _loggerMock.Verify(x => x.Log(
        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task ProcessAsync_WhenGraphHasAppsButNoFields_ItShouldUpdateGraph()
  {
    var item = FakeDataFactory.GraphQueueItem.Generate();
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _graphRepositoryMock
      .Setup(r => r.GetGraphAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(graph);

    _userRepositoryMock
      .Setup(r => r.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    _onspringClientMock
      .Setup(c => c.CanConnectAsync())
      .ReturnsAsync(true);

    _onspringClientMock
      .Setup(c => c.GetAppsAsync(It.IsAny<PagingRequest>()))
      .ReturnsAsync(new ApiResponse<GetPagedAppsResponse>()
      {
        StatusCode = HttpStatusCode.OK,
        Value = new()
        {
          Items =
          [
            new App(),
          ],
          PageNumber = 1,
          TotalPages = 1,
        }
      });

    _onspringClientMock
      .Setup(c => c.GetFieldsForAppAsync(It.IsAny<int>(), It.IsAny<PagingRequest>()))
      .ReturnsAsync(new ApiResponse<GetPagedFieldsResponse>()
      {
        StatusCode = HttpStatusCode.OK,
        Value = new()
        {
          Items = [],
          PageNumber = 1,
          TotalPages = 1,
        },
      });

    _hubContextMock
      .Setup(h => h.Clients.Group(It.IsAny<string>()))
      .Returns(Mock.Of<IGraphsClient>());

    await _graphProcessor.ProcessAsync(item);

    _graphRepositoryMock
      .Verify(
        x => x.UpdateGraphAsync(It.Is<Graph>(
          g =>
            g.Status == GraphStatus.Built &&
            g.Nodes.Count == 1 &&
            g.EdgesMap.Count == 1
          )
        ),
        Times.Once
      );
  }

  [Fact]
  public async Task ProcessAsync_WhenGraphHasAppsAndFields_ItShouldUpdateGraph()
  {
    var item = FakeDataFactory.GraphQueueItem.Generate();
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _graphRepositoryMock
      .Setup(r => r.GetGraphAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(graph);

    _userRepositoryMock
      .Setup(r => r.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    _onspringClientMock
      .Setup(c => c.CanConnectAsync())
      .ReturnsAsync(true);

    _onspringClientMock
      .Setup(c => c.GetAppsAsync(It.IsAny<PagingRequest>()))
      .ReturnsAsync(new ApiResponse<GetPagedAppsResponse>()
      {
        StatusCode = HttpStatusCode.OK,
        Value = new()
        {
          Items =
          [
            new App(),
          ],
          PageNumber = 1,
          TotalPages = 1,
        }
      });

    _onspringClientMock
      .Setup(c => c.GetFieldsForAppAsync(It.IsAny<int>(), It.IsAny<PagingRequest>()))
      .ReturnsAsync(new ApiResponse<GetPagedFieldsResponse>()
      {
        StatusCode = HttpStatusCode.OK,
        Value = new()
        {
          Items =
          [
            new Field(),
          ],
          PageNumber = 1,
          TotalPages = 1,
        },
      });

    _hubContextMock
      .Setup(h => h.Clients.Group(It.IsAny<string>()))
      .Returns(Mock.Of<IGraphsClient>());

    await _graphProcessor.ProcessAsync(item);

    _graphRepositoryMock
      .Verify(
        x => x.UpdateGraphAsync(It.Is<Graph>(
          g =>
            g.Status == GraphStatus.Built &&
            g.Nodes.Count == 1 &&
            g.EdgesMap.Count == 1
          )
        ),
        Times.Once
      );
  }

  [Fact]
  public async Task ProcessAsync_WhenGraphHasMultiplePagesOfAppsAndFields_ItShouldUpdateGraph()
  {
    var item = FakeDataFactory.GraphQueueItem.Generate();
    var graph = FakeDataFactory.Graph.Generate();
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _graphRepositoryMock
      .Setup(r => r.GetGraphAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(graph);

    _userRepositoryMock
      .Setup(r => r.GetUserByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(user);

    _onspringClientMock
      .Setup(c => c.CanConnectAsync())
      .ReturnsAsync(true);

    _onspringClientMock
      .SetupSequence(c => c.GetAppsAsync(It.IsAny<PagingRequest>()))
      .ReturnsAsync(new ApiResponse<GetPagedAppsResponse>()
      {
        StatusCode = HttpStatusCode.OK,
        Value = new()
        {
          Items =
          [
            new App() { Id = 1 },
          ],
          PageNumber = 1,
          TotalPages = 2,
        }
      })
      .ReturnsAsync(new ApiResponse<GetPagedAppsResponse>()
      {
        StatusCode = HttpStatusCode.OK,
        Value = new()
        {
          Items =
          [
            new App() { Id = 2 },
          ],
          PageNumber = 2,
          TotalPages = 2,
        }
      });

    _onspringClientMock
      .SetupSequence(c => c.GetFieldsForAppAsync(It.IsAny<int>(), It.IsAny<PagingRequest>()))
      .ReturnsAsync(new ApiResponse<GetPagedFieldsResponse>()
      {
        StatusCode = HttpStatusCode.OK,
        Value = new()
        {
          Items =
          [
            new Field() { AppId = 1, Type = FieldType.Reference },
          ],
          PageNumber = 1,
          TotalPages = 2,
        },
      })
      .ReturnsAsync(new ApiResponse<GetPagedFieldsResponse>()
      {
        StatusCode = HttpStatusCode.OK,
        Value = new()
        {
          Items =
          [
            new Field() { AppId = 2, Type = FieldType.Reference },
          ],
          PageNumber = 1,
          TotalPages = 2,
        },
      })
      .ReturnsAsync(new ApiResponse<GetPagedFieldsResponse>()
      {
        StatusCode = HttpStatusCode.OK,
        Value = new()
        {
          Items =
          [
            new Field() { AppId = 1, Type = FieldType.SurveyReference },
          ],
          PageNumber = 2,
          TotalPages = 2,
        },
      })
      .ReturnsAsync(new ApiResponse<GetPagedFieldsResponse>()
      {
        StatusCode = HttpStatusCode.OK,
        Value = new()
        {
          Items =
          [
            new Field() { AppId = 2, Type = FieldType.SurveyReference },
          ],
          PageNumber = 2,
          TotalPages = 2,
        },
      });

    _hubContextMock
      .Setup(h => h.Clients.Group(It.IsAny<string>()))
      .Returns(Mock.Of<IGraphsClient>());

    await _graphProcessor.ProcessAsync(item);

    _graphRepositoryMock
      .Verify(
        x => x.UpdateGraphAsync(It.Is<Graph>(
          g =>
            g.Status == GraphStatus.Built &&
            g.Nodes.Count == 2 &&
            g.EdgesMap.Count == 2 &&
            g.EdgesMap.Values.All(e => e.Count == 2)
          )
        ),
        Times.Once
      );
  }
}