namespace Server.API.Tests.Integration;

public class MongoGraphRepositoryTests : IClassFixture<TestDb>, IDisposable
{
  private readonly MongoDbContext _context;
  private readonly Mock<TimeProvider> _timeProvider = new();
  private readonly MongoGraphRepository _sut;

  public MongoGraphRepositoryTests(TestDb testDb)
  {
    _context = testDb.Context;
    _sut = new MongoGraphRepository(testDb.Context, _timeProvider.Object);
  }

  public void Dispose()
  {
    _context.Graphs.DeleteMany(g => true);
    GC.SuppressFinalize(this);
  }

  [Fact]
  public async Task CreateGraphAsync_WhenCalled_ItShouldCreateGraph()
  {
    var graph = FakeDataFactory.Graph.Generate();

    var result = await _sut.CreateGraphAsync(graph);

    result.Id
      .Should()
      .NotBeNullOrEmpty();

    var createdGraph = await _context.Graphs
      .Find(g => g.Id == result.Id)
      .SingleOrDefaultAsync();

    createdGraph
      .Should()
      .NotBeNull();
  }

  [Fact]
  public async Task GetGraphByNameAsync_WhenGraphExists_ItShouldReturnGraph()
  {
    var testGraph = FakeDataFactory.Graph.Generate();

    await _context.Graphs.InsertOneAsync(testGraph);

    var result = await _sut.GetGraphByNameAsync(testGraph.Name, testGraph.UserId);

    result
      .Should()
      .NotBeNull();

    result?.Name
      .Should()
      .Be(testGraph.Name);
  }

  [Fact]
  public async Task GetGraphByNameAsync_WhenGraphDoesNotExist_ItShouldReturnNull()
  {
    var testGraph = FakeDataFactory.Graph.Generate();

    var result = await _sut.GetGraphByNameAsync(testGraph.Name, testGraph.UserId);

    result
      .Should()
      .BeNull();
  }

  [Fact]
  public async Task GetGraphsAsync_WhenNoGraphsExist_ItShouldReturnEmptyPage()
  {
    var pageNumber = 1;
    var pageSize = 10;

    var result = await _sut.GetGraphsAsync(pageNumber, pageSize, "userId");

    result
      .Should()
      .NotBeNull();

    result.Should().BeEquivalentTo(new Page<Graph>(
      pageNumber: pageNumber,
      pageSize: pageSize,
      totalCount: 0,
      data: []
    ));
  }

  [Fact]
  public async Task GetGraphsAsync_WhenGraphsExist_ItShouldReturnPageOfGraphs()
  {
    var userId = "userId";
    var pageNumber = 1;
    var pageSize = 5;

    var testGraphs = FakeDataFactory.Graph
      .Generate(10)
      .Select(g =>
      {
        g.UserId = userId;
        return g;
      })
      .ToList();

    await _context.Graphs.InsertManyAsync(testGraphs);

    var result = await _sut.GetGraphsAsync(pageNumber, pageSize, userId);

    result
      .Should()
      .NotBeNull();

    result.TotalCount
      .Should()
      .Be(testGraphs.Count);

    result.TotalPages
      .Should()
      .Be(2);

    result.PageCount
      .Should()
      .Be(testGraphs.Count / 2);

    result.PageNumber
      .Should()
      .Be(pageNumber);

    result.Data
      .Should()
      .HaveCount(testGraphs.Count / 2);
  }

  [Fact]
  public async Task GetGraphAsync_WhenGraphExists_ItShouldReturnGraph()
  {
    var testGraph = FakeDataFactory.Graph.Generate();

    await _context.Graphs.InsertOneAsync(testGraph);

    var result = await _sut.GetGraphAsync(testGraph.Id, testGraph.UserId);

    result
      .Should()
      .NotBeNull();

    result?.Id
      .Should()
      .Be(testGraph.Id);
  }

  [Fact]
  public async Task GetGraphAsync_WhenGraphDoesNotExist_ItShouldReturnNull()
  {
    var testGraph = FakeDataFactory.Graph.Generate();

    var result = await _sut.GetGraphAsync(testGraph.Id, testGraph.UserId);

    result
      .Should()
      .BeNull();
  }
}