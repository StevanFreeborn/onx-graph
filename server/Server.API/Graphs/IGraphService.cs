namespace Server.API.Graphs;

/// <summary>
/// A service for managing graphs.
/// </summary>
interface IGraphService
{
  /// <summary>
  /// Adds a new graph.
  /// </summary>
  /// <param name="graph">The <see cref="Graph"/> to add.</param>
  /// <returns>The added graph as a <see cref="Graph"/> instance.</returns>
  Task<Result<Graph>> AddGraphAsync(Graph graph);

  /// <summary>
  /// Gets a graph by id.
  /// </summary>
  /// <param name="id">The id of the graph to get.</param>
  /// <param name="userId">The id of the user who owns the graph.</param>
  /// <returns>The graph as a <see cref="Graph"/> instance.</returns>
  Task<Result<Graph>> GetGraphAsync(string id, string userId);

  /// <summary>
  /// Gets a page of graphs.
  /// </summary>
  /// <param name="pageNumber">The page number to get.</param>
  /// <param name="pageSize">The number of items per page.</param>
  /// <param name="userId">The id of the user who owns the graphs.</param>
  /// <returns>A page of graphs as a <see cref="Page{Graph}"/> instance.</returns>
  Task<Result<Page<Graph>>> GetGraphsAsync(int pageNumber, int pageSize, string userId);

  /// <summary>
  /// Deletes a graph by id.
  /// </summary>
  /// <param name="id">The id of the graph to delete.</param>
  Task<Result> DeleteGraphAsync(string id);
}