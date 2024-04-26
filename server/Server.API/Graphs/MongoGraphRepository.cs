namespace Server.API.Graphs;

/// <summary>
/// A repository for managing graphs in MongoDB.
/// </summary>
/// <inheritdoc cref="IGraphRepository"/>
class MongoGraphRepository(MongoDbContext context) : IGraphRepository
{
  private const string CountFacetName = "count";
  private const string DataFacetName = "data";
  private readonly MongoDbContext _context = context;

  public async Task<Graph> CreateGraphAsync(Graph graph)
  {
    await _context.Graphs.InsertOneAsync(graph);
    return graph;
  }

  public async Task<Graph?> GetGraphByNameAsync(string name, string userId)
  {
    return await _context.Graphs
      .Find(g => g.Name == name && g.UserId == userId)
      .FirstOrDefaultAsync();
  }

  public async Task<Page<Graph>> GetGraphsAsync(int pageNumber, int pageSize, string userId)
  {
    var countFacet = AggregateFacet.Create(CountFacetName,
      PipelineDefinition<Graph, AggregateCountResult>.Create(
        new[]
        {
            PipelineStageDefinitionBuilder.Count<Graph>()
        }
      )
    );

    var dataFacet = AggregateFacet.Create(DataFacetName,
      PipelineDefinition<Graph, Graph>.Create(
        new[]
        {
          PipelineStageDefinitionBuilder.Skip<Graph>((pageNumber - 1) * pageSize),
          PipelineStageDefinitionBuilder.Limit<Graph>(pageSize),
        }
      )
    );

    var aggregate = await _context.Graphs.Aggregate()
      .Match(g => g.UserId == userId)
      .SortByDescending(g => g.CreatedAt)
      .Facet(countFacet, dataFacet)
      .FirstOrDefaultAsync();

    var countOutput = aggregate
      .Facets
      .First(facet => facet.Name == CountFacetName)
      .Output<AggregateCountResult>();

    if (countOutput.Count == 0)
    {
      return new Page<Graph>(
        pageNumber: pageNumber,
        pageSize: pageSize,
        totalCount: 0,
        data: []
      );
    }

    var count = countOutput[0].Count;

    var data = aggregate
      .Facets
      .First(facet => facet.Name == DataFacetName)
      .Output<Graph>();

    return new Page<Graph>(
      pageNumber: pageNumber,
      pageSize: pageSize,
      totalCount: count,
      data: [.. data]
    );
  }
}