namespace Server.API.Graphs;

static class GraphsController
{
  public static async Task<IResult> AddGraph([AsParameters] AddGraphRequest request)
  {
    var userId = request.HttpContext.GetUserId();

    if (userId is null)
    {
      return Results.Unauthorized();
    }

    var validationResult = await request.Validator.ValidateAsync(request.Dto);

    if (validationResult.IsValid is false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    return Results.Created();
  }
}