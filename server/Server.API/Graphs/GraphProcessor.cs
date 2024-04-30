
namespace Server.API.Graphs;

class GraphProcessor(
  IServiceScopeFactory serviceScopeFactory,
  IHubContext<GraphsHub, IGraphsClient> hubContext,
  ILogger<GraphProcessor> logger
) : IGraphProcessor
{
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
  private readonly IHubContext<GraphsHub, IGraphsClient> _hubContext = hubContext;
  private readonly ILogger<GraphProcessor> _logger = logger;

  public async Task ProcessAsync(GraphQueueItem item)
  {
    _logger.LogInformation("Processing item {ItemId} for graph {GraphId}", item.Id, item.GraphId);

    using var scope = _serviceScopeFactory.CreateScope();
    var graphRepository = scope.ServiceProvider.GetRequiredService<IGraphRepository>();

    var graph = await graphRepository.GetGraphAsync(item.GraphId, item.UserId);

    if (graph is null)
    {
      _logger.LogWarning("Graph {GraphId} belong to user {UserId} not found for item {ItemId}", item.GraphId, item.UserId, item.Id);
      return;
    }

    var groupId = $"{graph.UserId}-{graph.Id}";

    await _hubContext.Clients.Group(groupId).ReceiveUpdate("Building graph...");

    await Task.Delay(5000);

    await _hubContext.Clients.Group(groupId).ReceiveUpdate("Fetching apps for the graph...");

    await Task.Delay(5000);

    await _hubContext.Clients.Group(groupId).ReceiveUpdate("Fetching fields for the graph...");

    await Task.Delay(5000);

    await _hubContext.Clients.Group(groupId).ReceiveUpdate("Building graph...");

    await Task.Delay(5000);

    await _hubContext.Clients.Group(groupId).ReceiveUpdate("Graph built successfully!");

    _logger.LogInformation("Item {ItemId} for graph {GraphId} processed successfully", item.Id, item.GraphId);
  }
}