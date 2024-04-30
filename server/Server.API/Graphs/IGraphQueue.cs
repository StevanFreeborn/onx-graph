namespace Server.API.Graphs;

interface IGraphQueue
{
  Task EnqueueAsync(GraphQueueItem item);
  Task<GraphQueueItem> DequeueAsync();
}