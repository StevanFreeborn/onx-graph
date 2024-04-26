namespace Server.API.Graphs;

class Page<T>(
  int pageNumber,
  int pageSize,
  long totalCount,
  List<T> data
) where T : class
{
  public int PageCount { get; set; } = data.Count;
  public int PageNumber { get; set; } = pageNumber;
  public int TotalPages { get; set; } = totalCount is 0 ? 1 : (int)Math.Ceiling((double)totalCount / pageSize);
  public long TotalCount { get; set; } = totalCount;
  public List<T> Data { get; set; } = data;
}