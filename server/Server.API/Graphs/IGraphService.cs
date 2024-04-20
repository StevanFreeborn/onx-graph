namespace Server.API.Graphs;

interface IGraphService
{
  Task<Result<Graph>> AddGraph(Graph graph);
}