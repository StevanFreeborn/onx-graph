namespace Server.API.Graphs;

/// <summary>
/// Represents a response to adding a graph
/// </summary>
record AddGraphResponse(string Id);

record GraphDto
{
  public string Id { get; init; }
  public string Name { get; init; }
  public DateTime CreatedAt { get; init; }
  public DateTime UpdatedAt { get; init; }
  public GraphStatus Status { get; init; }

  public GraphDto(Graph graph)
  {
    Id = graph.Id;
    Name = graph.Name;
    CreatedAt = graph.CreatedAt;
    UpdatedAt = graph.UpdatedAt;
    Status = graph.Status;
  }
}

record GetGraphsResponse
{
  public int PageCount { get; init; }
  public int PageNumber { get; init; }
  public int TotalPages { get; init; }
  public long TotalCount { get; init; }
  public List<GraphDto> Data { get; init; }

  public GetGraphsResponse(Page<Graph> page)
  {
    PageCount = page.PageCount;
    PageNumber = page.PageNumber;
    TotalPages = page.TotalPages;
    TotalCount = page.TotalCount;
    Data = page.Data.Select(g => new GraphDto(g)).ToList();
  }
}