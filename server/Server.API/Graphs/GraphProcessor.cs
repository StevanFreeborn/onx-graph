
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
    // TODO: Cleanup
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

      var apps = new ConcurrentBag<App>();
      var appsResponse = await onspringClient.GetAppsAsync();

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
        var remainingPages = Enumerable.Range(currentPage + 1, totalPages);

        await Parallel.ForEachAsync(remainingPages, async (page, token) =>
        {
          var response = await onspringClient.GetAppsAsync(new PagingRequest() { PageNumber = page });

          if (response.IsSuccessful is false)
          {
            _logger.LogWarning(
              "Unable to fetch page {Page} of apps for graph {GraphId}: {ResponseStatus} - {ResponseMessage}",
              page,
              graph.Id,
              response.StatusCode,
              response.Message
            );

            return;
          }

          appsResponse.Value.Items.ForEach(apps.Add);
        });
      }

      if (apps.IsEmpty)
      {
        _logger.LogWarning("No apps found for graph {GraphId}", graph.Id);
        throw new GraphProcessingException("No apps found for the graph.");
      }

      await SendUpdate(groupId, "Fetching fields for the graph...");

      var referenceFields = new ConcurrentBag<Field>();

      await Parallel.ForEachAsync(apps, async (app, token) =>
      {
        var fieldsResponse = await onspringClient.GetFieldsForAppAsync(app.Id);

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
          if (field.Type is FieldType.Reference or FieldType.SurveyReference)
          {
            referenceFields.Add(field);
          }
        });

        var currentPage = fieldsResponse.Value.PageNumber;
        var totalPages = fieldsResponse.Value.TotalPages;

        if (totalPages > currentPage)
        {
          var remainingPages = Enumerable.Range(currentPage + 1, totalPages);

          await Parallel.ForEachAsync(remainingPages, async (page, token) =>
          {
            var response = await onspringClient.GetFieldsForAppAsync(app.Id, new PagingRequest() { PageNumber = page });

            if (response.IsSuccessful is false)
            {
              _logger.LogWarning(
                "Unable to fetch page {Page} of fields for app {AppId} in graph {GraphId}: {ResponseStatus} - {ResponseMessage}",
                page,
                app.Id,
                graph.Id,
                response.StatusCode,
                response.Message
              );

              return;
            }

            fieldsResponse.Value.Items.ForEach(field =>
            {
              if (field.Type is FieldType.Reference or FieldType.SurveyReference)
              {
                referenceFields.Add(field);
              }
            });
          });
        }
      });

      if (referenceFields.IsEmpty)
      {
        _logger.LogWarning("No fields found for graph {GraphId}", graph.Id);
        throw new GraphProcessingException("No fields found for the graph.");
      }

      await _hubContext.Clients.Group(groupId).ReceiveUpdate("Updating graph...");

      var edgesMap = new Dictionary<string, List<Field>>();

      foreach (var app in apps)
      {
        edgesMap[app.Id.ToString()] = referenceFields.Where(f => f.AppId == app.Id).ToList();
      }

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

  private async Task SendUpdate(string groupId, string message)
  {
    await _hubContext.Clients.Group(groupId).ReceiveUpdate(message);
  }
}

class GraphProcessingException(string message) : Exception(message)
{
}