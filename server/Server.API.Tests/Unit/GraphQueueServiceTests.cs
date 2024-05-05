namespace Server.API.Tests.Unit;

public class GraphQueueServiceTests
{
  private readonly Mock<ILogger<GraphQueueService>> _loggerMock = new();
  private readonly Mock<IGraphQueue> _queueMock = new();
  private readonly Mock<IGraphProcessor> _processorMock = new();
  private readonly GraphQueueService _sut;

  public GraphQueueServiceTests()
  {
    _sut = new GraphQueueService(_loggerMock.Object, _queueMock.Object, _processorMock.Object);
  }

  [Fact]
  public async Task ProcessAsync_WhenCalledAndNoItem_ShouldNotProcessItem()
  {
    _queueMock
      .Setup(x => x.DequeueAsync())
      .ReturnsAsync(null as GraphQueueItem);

    await _sut.ProcessItemAsync(CancellationToken.None);

    _queueMock.Verify(x => x.DequeueAsync(), Times.Once);
    _processorMock.Verify(x => x.ProcessAsync(It.IsAny<GraphQueueItem>()), Times.Never);
  }

  [Fact]
  public async Task ProcessAsync_WhenCalledAndItem_ShouldProcessItem()
  {
    var item = new GraphQueueItem() with { CreatedAt = DateTime.UtcNow.AddSeconds(-5) };

    _queueMock
      .Setup(x => x.DequeueAsync())
      .ReturnsAsync(item);

    await _sut.ProcessItemAsync(CancellationToken.None);

    _queueMock.Verify(x => x.DequeueAsync(), Times.Once);
    _processorMock.Verify(x => x.ProcessAsync(item), Times.Once);
  }

  [Fact]
  public async Task ProcessAsync_WhenCalledAndItemAndProcessorThrows_ShouldLogError()
  {
    var item = new GraphQueueItem() with { CreatedAt = DateTime.UtcNow.AddSeconds(-5) };
    var exception = new Exception();

    _queueMock
      .Setup(x => x.DequeueAsync())
      .ReturnsAsync(item);

    _processorMock
      .Setup(x => x.ProcessAsync(item))
      .ThrowsAsync(exception);

    await _sut.ProcessItemAsync(CancellationToken.None);

    _queueMock.Verify(x => x.DequeueAsync(), Times.Once);
    _processorMock.Verify(x => x.ProcessAsync(item), Times.Once);
    _loggerMock.Verify(x => x.Log(
        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
      ),
      Times.Once
    );
  }
}