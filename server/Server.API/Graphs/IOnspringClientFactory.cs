namespace Server.API.Graphs;

interface IOnspringClientFactory
{
  IOnspringClient CreateClient(string apiKey);
}