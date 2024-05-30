namespace Server.API.Graphs;

class GraphAlreadyExistsError : Error
{
  internal GraphAlreadyExistsError(string name) : base($"Graph already exists with name: {name}")
  {
  }
}

class GraphNotFoundError : Error
{
  internal GraphNotFoundError(string id) : base($"Graph not found with id: {id}")
  {
  }
}