namespace Server.API.Tests.Integration.Fixtures;

public class TestDb : IDisposable
{
  private readonly MongoDbContainer _container;
  internal MongoDbContext Context { get; init; }

  public TestDb()
  {
    _container = new MongoDbBuilder().Build();
    _container.StartAsync().Wait();
    Context = new(
      Options.Create(new MongoDbOptions
      {
        ConnectionString = _container.GetConnectionString(),
        DatabaseName = "test",
      })
    );
  }

  public void Dispose()
  {
    _container.DisposeAsync().AsTask();
    GC.SuppressFinalize(this);
  }
}