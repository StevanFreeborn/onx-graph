namespace Server.API.Graphs;

/// <summary>
/// A service for managing graphs.
/// </summary>
/// <inheritdoc cref="IGraphService"/>
class GraphService(IGraphRepository graphRepository) : IGraphService
{
  private readonly IGraphRepository _graphRepository = graphRepository;

  public async Task<Result<Graph>> AddGraphAsync(Graph graph)
  {
    var existingGraph = await _graphRepository.GetGraphByNameAsync(graph.Name, graph.UserId);

    if (existingGraph is not null)
    {
      return Result.Fail(new GraphAlreadyExistsError(graph.Name));
    }

    var createdGraph = await _graphRepository.CreateGraphAsync(graph);
    return Result.Ok(createdGraph);
  }

  public async Task<Result<Page<Graph>>> GetGraphsAsync(int pageNumber, int pageSize, string userId)
  {
    var graphs = await _graphRepository.GetGraphsAsync(pageNumber, pageSize, userId);
    return Result.Ok(graphs);
  }
}