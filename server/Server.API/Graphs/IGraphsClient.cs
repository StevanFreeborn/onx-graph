namespace Server.API.Graphs;

/// <summary>
/// Interface for the GraphsHub client.
/// </summary>
public interface IGraphsClient
{
  /// <summary>
  /// Receives an update.
  /// </summary>
  Task ReceiveUpdate(string update);

  /// <summary>
  /// Sends a message.
  /// </summary>
  Task GraphBuilt();

  /// <summary>
  /// Sends a message.
  /// </summary>
  Task GraphError();
}