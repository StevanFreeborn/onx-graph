namespace Server.API.Graphs;

[Authorize]
class GraphsHub : Hub<IGraphsClient>
{
  public async Task JoinGraph(string graphId)
  {
    var userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);
    await Groups.AddToGroupAsync(Context.ConnectionId, $"{userId}-{graphId}");
    await Clients.Group($"{userId}-{graphId}").ReceiveUpdate("User joined the graph");
  }
}