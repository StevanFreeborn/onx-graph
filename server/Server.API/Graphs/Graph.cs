namespace Server.API.Graphs;

class Graph(string name, string apiKey)
{
  public string Id { get; set; } = string.Empty;
  public string Name { get; set; } = name;
  public string ApiKey { get; set; } = apiKey;

  public Graph(AddGraphDto dto) : this(dto.Name, dto.ApiKey) { }
}