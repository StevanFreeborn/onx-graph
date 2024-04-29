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
}