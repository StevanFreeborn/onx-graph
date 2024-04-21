namespace Server.API.Graphs;

/// <summary>
/// A repository for managing graphs.
/// </summary>
interface IGraphRepository
{
  /// <summary>
  /// Creates a new graph.
  /// </summary>
  /// <param name="graph">The graph to create.</param>
  /// <returns>The created graph as a <see cref="Graph"/> instance.</returns>
  Task<Graph> CreateGraphAsync(Graph graph);

  /// <summary>
  /// Gets a graph by its name.
  /// </summary>
  /// <param name="name">The name of the graph to get.</param>
  /// <param name="userId">The id of the user who owns the graph.</param>
  /// <returns>The graph as a <see cref="Graph"/> instance.</returns>
  Task<Graph?> GetGraphByNameAsync(string name, string userId);
}