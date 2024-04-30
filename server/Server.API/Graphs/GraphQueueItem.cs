namespace Server.API.Graphs;

record GraphQueueItem
{
  public string Id { get; init; } = Guid.NewGuid().ToString();
  public string GraphId { get; init; }
  public string UserId { get; init; }

  public GraphQueueItem(Graph graph)
  {
    GraphId = graph.Id;
    UserId = graph.UserId;
  }
}