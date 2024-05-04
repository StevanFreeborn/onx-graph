namespace Server.API.Graphs;

record GraphQueueItem
{
  public string Id { get; init; } = Guid.NewGuid().ToString();
  public string GraphId { get; init; } = string.Empty;
  public string UserId { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

  internal GraphQueueItem() { }

  public GraphQueueItem(Graph graph)
  {
    GraphId = graph.Id;
    UserId = graph.UserId;
  }
}