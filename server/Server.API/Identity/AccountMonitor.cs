

namespace Server.API.Identity;

/// <summary>
/// Service that will monitor for unverified accounts and delete them after a certain period of time.
/// </summary>
class AccountMonitor(
  ILogger<AccountMonitor> logger,
  TimeProvider timeProvider,
  MongoDbContext dbContext
) : IHostedService, IDisposable
{
  private readonly ILogger<AccountMonitor> _logger = logger;
  private readonly TimeProvider _timeProvider = timeProvider;
  private ITimer? _timer;
  private readonly MongoDbContext _dbContext = dbContext;

  /// <summary>
  /// Starts the account monitor.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Account monitor started");

    _timer = _timeProvider.CreateTimer(
      callback: async _ => await DeleteUnverifiedAccounts(),
      state: null,
      dueTime: TimeSpan.FromMinutes(15),
      period: TimeSpan.FromMinutes(15)
    );

    return Task.CompletedTask;
  }

  /// <summary>
  /// Stops the account monitor.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Account monitor stopped");

    _timer?.Change(Timeout.InfiniteTimeSpan, TimeSpan.Zero);

    return Task.CompletedTask;
  }

  /// <summary>
  /// Disposes of the account monitor.
  /// </summary>
  public void Dispose()
  {
    _timer?.Dispose();
    GC.SuppressFinalize(this);
  }

  private async Task DeleteUnverifiedAccounts()
  {
    _logger.LogInformation("Deleting unverified accounts");

    try
    {
      var userFilter = Builders<User>.Filter.And(
        Builders<User>.Filter.Lt(x => x.CreatedAt, _timeProvider.GetUtcNow().DateTime.AddHours(-48)),
        Builders<User>.Filter.Eq(x => x.IsVerified, false)
      );

      var usersToDelete = await _dbContext.Users.Find(userFilter).ToListAsync();

      var tokenFilter = Builders<BaseToken>.Filter.In(
        x => x.UserId,
        usersToDelete.Select(x => x.Id)
      );

      var usersToDeleteFilter = Builders<User>.Filter.In(
        x => x.Id,
        usersToDelete.Select(x => x.Id)
      );

      await _dbContext.Tokens.DeleteManyAsync(tokenFilter);

      var result = await _dbContext.Users.DeleteManyAsync(usersToDeleteFilter);

      _logger.LogInformation("Deleted {Count} unverified accounts", result.DeletedCount);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred while deleting unverified accounts");
    }
  }
}