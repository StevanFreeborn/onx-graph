
using System.Collections.Concurrent;

namespace Server.API.Graphs;

class GraphProcessor(
  IServiceScopeFactory serviceScopeFactory,
  IHubContext<GraphsHub, IGraphsClient> hubContext,
  ILogger<GraphProcessor> logger,
  IEncryptionService encryptionService,
  IOnspringClientFactory onspringClientFactory
) : IGraphProcessor
{
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
  private readonly IHubContext<GraphsHub, IGraphsClient> _hubContext = hubContext;
  private readonly ILogger<GraphProcessor> _logger = logger;
  private readonly IEncryptionService _encryptionService = encryptionService;
  private readonly IOnspringClientFactory _onspringClientFactory = onspringClientFactory;

  public async Task ProcessAsync(GraphQueueItem item)
  {
    _logger.LogInformation("Processing item {ItemId} for graph {GraphId}", item.Id, item.GraphId);

    using var scope = _serviceScopeFactory.CreateScope();
    var graphRepository = scope.ServiceProvider.GetRequiredService<IGraphRepository>();
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var graph = await graphRepository.GetGraphAsync(item.GraphId, item.UserId);

    if (graph is null)
    {
      _logger.LogWarning("Graph {GraphId} belonging to user {UserId} not found for item {ItemId}", item.GraphId, item.UserId, item.Id);
      return;
    }

    var user = await userRepository.GetUserByIdAsync(graph.UserId);

    if (user is null)
    {
      _logger.LogWarning("User {UserId} not found for item {ItemId}", graph.UserId, item.Id);
      return;
    }

    var groupId = $"{graph.UserId}-{graph.Id}";

    try
    {
      await SendUpdate(groupId, "Building graph...");

      await SendUpdate(groupId, "Decrypting API key...");
      var graphApiKey = await _encryptionService.DecryptForUserAsync(graph.ApiKey, user);

      await SendUpdate(groupId, "Creating Onspring client...");
      var onspringClient = _onspringClientFactory.CreateClient(graphApiKey);

      await SendUpdate(groupId, "Checking Onspring connection...");

      var canConnect = await onspringClient.CanConnectAsync();

      if (canConnect is false)
      {
        _logger.LogWarning("Unable to connect to Onspring for graph {GraphId}", graph.Id);
        throw new GraphProcessingException("Unable to connect to Onspring.");
      }

      await SendUpdate(groupId, "Fetching apps for the graph...");

      var apps = await GetAppsAsync(onspringClient, graph);

      if (apps.IsEmpty)
      {
        _logger.LogWarning("No apps found for graph {GraphId}", graph.Id);
        throw new GraphProcessingException("No apps found for the graph.");
      }

      await SendUpdate(groupId, "Fetching fields for the graph...");

      var fields = await GetFieldsAsync(onspringClient, graph, apps);

      await _hubContext.Clients.Group(groupId).ReceiveUpdate("Updating graph...");

      var edgesMap = apps.ToDictionary(
        app => app.Id.ToString(),
        app => fields.Where(field => field.AppId == app.Id).ToList()
      );

      graph.Nodes = [.. apps];
      graph.EdgesMap = edgesMap;
      graph.Status = GraphStatus.Built;
      await graphRepository.UpdateGraphAsync(graph);

      await _hubContext.Clients.Group(groupId).GraphBuilt();

      _logger.LogInformation("Item {ItemId} for graph {GraphId} processed successfully", item.Id, item.GraphId);
    }
    catch (Exception ex)
    {
      if (ex is GraphProcessingException)
      {
        await SendUpdate(groupId, ex.Message);
      }

      await _hubContext.Clients.Group(groupId).GraphError();

      graph.Status = GraphStatus.NotBuilt;
      await graphRepository.UpdateGraphAsync(graph);

      _logger.LogError(ex, "Error processing item {ItemId} for graph {GraphId}", item.Id, item.GraphId);
    }
  }

  private async Task<ConcurrentBag<App>> GetAppsAsync(IOnspringClient client, Graph graph)
  {
    var apps = new ConcurrentBag<App>();

    var appsResponse = await client.GetAppsAsync(new PagingRequest() { PageNumber = 1 });

    if (appsResponse.IsSuccessful is false)
    {
      _logger.LogWarning(
        "Unable to fetch page 1 of apps for graph {GraphId}: {ResponseStatus} - {ResponseMessage}",
        graph.Id,
        appsResponse.StatusCode,
        appsResponse.Message
      );

      throw new GraphProcessingException("Unable to fetch apps for the graph.");
    }

    appsResponse.Value.Items.ForEach(apps.Add);

    var currentPage = appsResponse.Value.PageNumber;
    var totalPages = appsResponse.Value.TotalPages;

    if (totalPages > currentPage)
    {
      var nextPage = currentPage + 1;
      var numRemaining = totalPages - 1;

      var remainingAppRequests = Enumerable
        .Range(nextPage, numRemaining)
        .Select(page => new PagingRequest() { PageNumber = page });

      await Parallel.ForEachAsync(remainingAppRequests, async (pageRequest, token) =>
      {
        var response = await client.GetAppsAsync(pageRequest);

        if (response.IsSuccessful is false)
        {
          _logger.LogWarning(
            "Unable to fetch page {Page} of apps for graph {GraphId}: {ResponseStatus} - {ResponseMessage}",
            pageRequest.PageNumber,
            graph.Id,
            response.StatusCode,
            response.Message
          );

          return;
        }

        response.Value.Items.ForEach(apps.Add);
      });
    }

    return apps;
  }

  private async Task<ConcurrentBag<ReferenceField>> GetFieldsAsync(IOnspringClient client, Graph graph, ConcurrentBag<App> apps)
  {
    var fields = new ConcurrentBag<ReferenceField>();

    await Parallel.ForEachAsync(apps, async (app, token) =>
    {
      var fieldsResponse = await client.GetFieldsForAppAsync(app.Id, new PagingRequest() { PageNumber = 1 });

      if (fieldsResponse.IsSuccessful is false)
      {
        _logger.LogWarning(
          "Unable to fetch fields for app {AppId} in graph {GraphId}: {ResponseStatus} - {ResponseMessage}",
          app.Id,
          graph.Id,
          fieldsResponse.StatusCode,
          fieldsResponse.Message
        );

        return;
      }

      fieldsResponse.Value.Items.ForEach(field =>
      {
        if (field is ReferenceField reference)
        {
          fields.Add(reference);
        }
      });

      var currentPage = fieldsResponse.Value.PageNumber;
      var totalPages = fieldsResponse.Value.TotalPages;

      if (totalPages > currentPage)
      {
        var nextPage = currentPage + 1;
        var numRemaining = totalPages - 1;
        var remainingFieldRequests = Enumerable
          .Range(nextPage, numRemaining)
          .Select(page => new PagingRequest() { PageNumber = page });

        await Parallel.ForEachAsync(remainingFieldRequests, async (pageRequest, token) =>
        {
          var response = await client.GetFieldsForAppAsync(app.Id, pageRequest);

          if (response.IsSuccessful is false)
          {
            _logger.LogWarning(
              "Unable to fetch page {Page} of fields for app {AppId} in graph {GraphId}: {ResponseStatus} - {ResponseMessage}",
              pageRequest.PageNumber,
              app.Id,
              graph.Id,
              response.StatusCode,
              response.Message
            );

            return;
          }

          response.Value.Items.ForEach(field =>
          {
            if (field is ReferenceField reference)
            {
              fields.Add(reference);
            }
          });
        });
      }
    });

    return fields;
  }

  private async Task SendUpdate(string groupId, string message)
  {
    await _hubContext.Clients.Group(groupId).ReceiveUpdate(message);
  }
}

class GraphProcessingException(string message) : Exception(message)
{
}