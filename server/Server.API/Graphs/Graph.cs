namespace Server.API.Graphs;

/// <summary>
/// Represents a graph.
/// </summary>
class Graph(string name, string apiKey, User user)
{
  public string Id { get; set; } = string.Empty;
  public string UserId { get; set; } = user.Id;
  public string Name { get; set; } = name;
  public string ApiKey { get; set; } = apiKey;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  public GraphStatus Status { get; set; } = GraphStatus.Building;
  public List<App> Nodes { get; set; } = [];
  public Dictionary<string, List<Field>> EdgesMap { get; set; } = [];

  public Graph(AddGraphDto dto, User user) : this(dto.Name, dto.ApiKey, user) { }
  internal Graph() : this(string.Empty, string.Empty, new User()) { }
}

/// <summary>
/// Represents the status of a graph.
/// </summary>
enum GraphStatus
{
  NotBuilt,
  Building,
  Built,
}