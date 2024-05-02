namespace Server.API.Graphs;

class ChannelGraphQueue : IGraphQueue
{
  private readonly Channel<GraphQueueItem> _channel = Channel.CreateUnbounded<GraphQueueItem>();

  public async Task EnqueueAsync(GraphQueueItem item) => await _channel.Writer.WriteAsync(item);
  public async Task<GraphQueueItem?> DequeueAsync() => await _channel.Reader.ReadAsync();
}