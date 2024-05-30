
namespace Server.API.Tests.Integration.Fixtures;

public class TestDb : IAsyncLifetime
{
  private readonly MongoDbContainer _container;
  internal MongoDbContext Context { get; set; } = null!;

  public TestDb()
  {
    _container = new MongoDbBuilder().Build();
  }

  public async Task InitializeAsync()
  {
    await _container.StartAsync();
    Context = new(
      Options.Create(new MongoDbOptions
      {
        ConnectionString = _container.GetConnectionString(),
        DatabaseName = "test",
      })
    );
  }

  async Task IAsyncLifetime.DisposeAsync() => await _container.DisposeAsync();
}