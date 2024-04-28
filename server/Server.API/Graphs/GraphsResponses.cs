using System.Text.Json.Serialization;

namespace Server.API.Graphs;

/// <summary>
/// Represents a response to adding a graph
/// </summary>
record AddGraphResponse(string Id);

record GraphDto
{
  public string Id { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
  public GraphStatus Status { get; init; } = GraphStatus.NotBuilt;

  [JsonConstructor]
  internal GraphDto()
  {
  }

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
  public List<GraphDto> Data { get; init; } = [];

  [JsonConstructor]
  internal GetGraphsResponse()
  {
  }

  public GetGraphsResponse(Page<Graph> page)
  {
    PageCount = page.PageCount;
    PageNumber = page.PageNumber;
    TotalPages = page.TotalPages;
    TotalCount = page.TotalCount;
    Data = page.Data.Select(g => new GraphDto(g)).ToList();
  }
}