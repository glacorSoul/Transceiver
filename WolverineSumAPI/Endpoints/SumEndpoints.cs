using Microsoft.AspNetCore.Http;
using Wolverine.Http;

namespace WolverineSumAPI.Endpoints;

public static class SumEndpoints
{
    [WolverineGet("/sum/{a}/{b}")]
    public static IResult Sum(int a, int b)
    {
        return Results.Ok(a + b);
    }
}
