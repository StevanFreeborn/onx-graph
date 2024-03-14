namespace Server.API.Tests.Integration.Fixtures;

public class MailHogService(HttpClient client)
{
  private readonly HttpClient _client = client;

  public async Task<MailHogSearchResult> SearchEmailAsync(MailHogSearchRequest req)
  {
    var kind = req.Kind switch
    {
      MailHogSearchKind.To => "to",
      MailHogSearchKind.From => "from",
      MailHogSearchKind.Containing => "containing",
      _ => throw new ArgumentException($"Invalid request: {nameof(req.Kind)} is not to, from, or containing.", nameof(req))
    };

    var url = $"/api/v2/search?kind={kind}&query={req.Query}&start={req.Start}&limit={req.Limit}";
    var response = await _client.GetFromJsonAsync<MailHogSearchResult>(url);
    return response ?? new MailHogSearchResult();
  }
}

public class MailHogSearchResult
{
  public List<MailHogMessage> Items { get; init; } = [];
  public int Total { get; init; } = 0;
  public int Start { get; init; } = 0;
  public int Count { get; init; } = 0;
}

public class MailHogMessage
{
  public string Id { get; init; } = string.Empty;
  public MailHogToOrFrom From { get; init; } = new();
  public List<MailHogToOrFrom> To { get; init; } = [];
  public Dictionary<string, string> Headers { get; init; } = [];
  public int Size { get; init; }
  public DateTime Created { get; init; }
}

public class MailHogToOrFrom
{
  public List<string> Relays { get; init; } = [];
  public string Mailbox { get; init; } = string.Empty;
  public string Domain { get; init; } = string.Empty;
  public string @Params { get; init; } = string.Empty;
}


public class MailHogSearchRequest
{
  public MailHogSearchKind Kind { get; init; }
  public string Query { get; init; }
  public int Start { get; init; } = 0;
  public int Limit { get; init; } = 50;

  public MailHogSearchRequest(MailHogSearchKind kind, string query)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(query, nameof(query));

    Kind = kind;
    Query = query;
  }
}

public enum MailHogSearchKind
{
  To,
  From,
  Containing,
}