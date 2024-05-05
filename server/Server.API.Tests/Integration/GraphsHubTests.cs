namespace Server.API.Tests.Integration;

public class GraphsHubTests(TestServerFactory serverFactory) : IntegrationTest(serverFactory), IDisposable
{
  public void Dispose()
  {
    GC.SuppressFinalize(this);
  }

  [Fact]
  public async Task ReceiveUpdate_WhenCalled_ItShouldSendUpdateToClientsInGroup()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var graph = FakeDataFactory.Graph.Generate();
    var update = "update";
    var updateReceived = string.Empty;

    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, user.Id))
      .Build();

    var connection = await GetGraphsHubConnectionAsync(userJwtToken);
    var groupId = $"{user.Id}-{graph.Id}";

    await connection.SendAsync(nameof(GraphsHub.JoinGraph), graph.Id);

    await Task.Delay(1000);

    connection.On<string>(nameof(IGraphsClient.ReceiveUpdate), u => updateReceived = u);

    var hubContext = _factory.Services.GetRequiredService<IHubContext<GraphsHub, IGraphsClient>>();

    await hubContext.Clients.Group(groupId).ReceiveUpdate(update);

    await Task.Delay(1000);

    updateReceived.Should().Be(update);
  }
}