namespace Server.API.Tests.Integration.Fixtures;

public class TestDb : IDisposable
{
  private readonly MongoDbContainer _container = new MongoDbBuilder().Build();
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
    _container.DisposeAsync().AsTask().Wait();
    GC.SuppressFinalize(this);
  }
}