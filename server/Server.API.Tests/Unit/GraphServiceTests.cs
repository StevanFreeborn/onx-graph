namespace Server.API.Tests.Unit;

public class GraphServiceTests
{
  private readonly Mock<IGraphRepository> _graphRepositoryMock = new();
  private readonly GraphService _sut;

  public GraphServiceTests()
  {
    _sut = new GraphService(_graphRepositoryMock.Object);
  }

  [Fact]
  public async Task AddGraph_WhenCalledWithExistingGraph_ItShouldReturnGraphAlreadyExistsError()
  {
    var graph = FakeDataFactory.Graph.Generate();

    _graphRepositoryMock
      .Setup(x => x.GetGraphByNameAsync(graph.Name, graph.UserId))
      .ReturnsAsync(graph);

    var result = await _sut.AddGraphAsync(graph);

    result.Errors.Should().ContainSingle(e => e is GraphAlreadyExistsError);
  }

  [Fact]
  public async Task AddGraph_WhenCalled_ItShouldCallRepositoryAndReturnCreatedGraph()
  {
    var graph = FakeDataFactory.Graph.Generate();

    _graphRepositoryMock
      .Setup(x => x.GetGraphByNameAsync(graph.Name, graph.UserId))
      .ReturnsAsync(null as Graph);

    _graphRepositoryMock
      .Setup(x => x.CreateGraphAsync(graph))
      .ReturnsAsync(graph);

    var result = await _sut.AddGraphAsync(graph);

    result.Value.Should().BeEquivalentTo(graph);

    _graphRepositoryMock.Verify(x => x.CreateGraphAsync(graph), Times.Once);
  }

  [Fact]
  public async Task GetGraphs_WhenCalled_ItShouldCallRepositoryAndReturnGraphs()
  {
    var pageNumber = 1;
    var pageSize = 10;
    var userId = "userId";
    var graphs = FakeDataFactory.Graph.Generate(10);
    var page = new Page<Graph>(pageNumber, pageSize, 10, graphs);

    _graphRepositoryMock
      .Setup(x => x.GetGraphsAsync(pageNumber, pageSize, userId))
      .ReturnsAsync(page);

    var result = await _sut.GetGraphsAsync(pageNumber, pageSize, userId);

    result.Value.Should().BeEquivalentTo(page);

    _graphRepositoryMock.Verify(x => x.GetGraphsAsync(pageNumber, pageSize, userId), Times.Once);
  }
}