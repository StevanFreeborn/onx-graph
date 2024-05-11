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
  /// Gets a graph by its id.
  /// </summary>
  /// <param name="id">The id of the graph to get.</param>
  /// <param name="userId">The id of the user who owns the graph.</param>
  /// <returns>The graph as a <see cref="Graph"/> instance.</returns>
  Task<Graph?> GetGraphAsync(string id, string userId);

  /// <summary>
  /// Gets a graph by its name.
  /// </summary>
  /// <param name="name">The name of the graph to get.</param>
  /// <param name="userId">The id of the user who owns the graph.</param>
  /// <returns>The graph as a <see cref="Graph"/> instance.</returns>
  Task<Graph?> GetGraphByNameAsync(string name, string userId);

  /// <summary>
  /// Gets a page of graphs.
  /// </summary>
  /// <param name="pageNumber">The page number to get.</param>
  /// <param name="pageSize">The number of items per page.</param>
  /// <param name="userId">The id of the user who owns the graphs.</param>
  /// <returns>A page of graphs as a <see cref="Page{Graph}"/> instance.</returns>
  Task<Page<Graph>> GetGraphsAsync(int pageNumber, int pageSize, string userId);

  /// <summary>
  /// Updates a graph.
  /// </summary>
  /// <param name="graph">The graph to update.</param>
  /// <returns>A <see cref="Task"/>.</returns>
  Task UpdateGraphAsync(Graph graph);

  /// <summary>
  /// Deletes a graph by its id.
  /// </summary>
  /// <param name="id">The id of the graph to delete.</param>
  /// <returns>A <see cref="Task"/>.</returns>
  Task DeleteGraphAsync(string id);
}