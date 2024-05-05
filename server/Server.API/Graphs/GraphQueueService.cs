namespace Server.API.Graphs;

class GraphQueueService(
  ILogger<GraphQueueService> logger,
  IGraphQueue queue,
  IGraphProcessor processor
) : BackgroundService
{
  private readonly ILogger<GraphQueueService> _logger = logger;
  private readonly IGraphQueue _queue = queue;
  private readonly IGraphProcessor _processor = processor;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (stoppingToken.IsCancellationRequested is false)
    {
      await ProcessItemAsync(stoppingToken);
    }
  }

  internal async Task ProcessItemAsync(CancellationToken stoppingToken)
  {
    var item = await _queue.DequeueAsync();

    if (item is null)
    {
      return;
    }

    // if the item was created less than 3 seconds ago, re-enqueue it
    // trying to prevent item being processed before user has had a
    // chance to connect for updates
    if (item.CreatedAt.AddSeconds(3) > DateTime.UtcNow)
    {
      await _queue.EnqueueAsync(item);
      _logger.LogInformation("Waiting to process item {ItemId} for graph {GraphId}", item.Id, item.GraphId);
      return;
    }

    _ = Task.Run(async () =>
    {
      try
      {
        await _processor.ProcessAsync(item);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error processing item {ItemId} for graph {GraphId}", item.Id, item.GraphId);
      }
    }, stoppingToken);

    _logger.LogInformation("Dequeued item {ItemId} for processing graph {GraphId}", item.Id, item.GraphId);
  }
}