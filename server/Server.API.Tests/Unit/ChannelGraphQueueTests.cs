namespace Server.API.Tests.Unit;

public class ChannelGraphQueueTests
{
  [Fact]
  public async Task EnqueueAsync_WhenCalled_ShouldEnqueueItem()
  {
    var item = new GraphQueueItem();
    var queue = new ChannelGraphQueue();

    await queue.EnqueueAsync(item);

    var queueItem = await queue.DequeueAsync();

    queueItem.Should().BeSameAs(item);
  }
}