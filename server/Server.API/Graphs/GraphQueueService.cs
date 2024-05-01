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
      var item = await _queue.DequeueAsync();

      if (item is null)
      {
        continue;
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
}