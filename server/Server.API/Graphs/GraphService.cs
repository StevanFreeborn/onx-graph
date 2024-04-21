namespace Server.API.Graphs;

class GraphService(IGraphRepository graphRepository) : IGraphService
{
  private readonly IGraphRepository _graphRepository = graphRepository;

  public async Task<Result<Graph>> AddGraph(Graph graph)
  {
    var existingGraph = await _graphRepository.GetGraphByNameAsync(graph.Name, graph.UserId);

    if (existingGraph is not null)
    {
      return Result.Fail(new GraphAlreadyExistsError(graph.Name));
    }

    var createdGraph = await _graphRepository.CreateGraphAsync(graph);
    return Result.Ok(createdGraph);
  }
}