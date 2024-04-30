namespace Server.API.Graphs;

interface IGraphProcessor
{
  Task ProcessAsync(GraphQueueItem item);
}