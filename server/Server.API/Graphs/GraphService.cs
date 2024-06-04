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

  public async Task<Result> DeleteGraphAsync(string id)
  {
    await _graphRepository.DeleteGraphAsync(id);
    return Result.Ok();
  }

  public async Task<Result<Graph>> GetGraphAsync(string id, string userId)
  {
    var graph = await _graphRepository.GetGraphAsync(id, userId);

    if (graph is null)
    {
      return Result.Fail(new GraphNotFoundError(id));
    }

    return Result.Ok(graph);
  }

  public async Task<Result<Page<Graph>>> GetGraphsAsync(int pageNumber, int pageSize, string userId)
  {
    var graphs = await _graphRepository.GetGraphsAsync(pageNumber, pageSize, userId);
    return Result.Ok(graphs);
  }

  public async Task<Result<Graph>> UpdateGraphAsync(Graph graph)
  {
    var existingGraph = await _graphRepository.GetGraphByNameAsync(graph.Name, graph.UserId);

    if (existingGraph is not null && existingGraph.Id != graph.Id)
    {
      return Result.Fail(new GraphAlreadyExistsError(graph.Name));
    }

    await _graphRepository.UpdateGraphAsync(graph);
    return Result.Ok();
  }
}