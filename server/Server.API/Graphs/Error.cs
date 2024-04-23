namespace Server.API.Graphs;

class GraphAlreadyExistsError : Error
{
  internal GraphAlreadyExistsError(string name) : base($"Graph already exists with name: {name}")
  {
  }
}