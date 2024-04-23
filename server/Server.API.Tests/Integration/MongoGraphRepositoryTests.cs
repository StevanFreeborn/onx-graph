namespace Server.API.Tests.Integration;

public class MongoGraphRepositoryTests : IClassFixture<TestDb>, IDisposable
{
  private readonly MongoDbContext _context;
  private readonly MongoGraphRepository _sut;

  public MongoGraphRepositoryTests(TestDb testDb)
  {
    _context = testDb.Context;
    _sut = new MongoGraphRepository(testDb.Context);
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
}