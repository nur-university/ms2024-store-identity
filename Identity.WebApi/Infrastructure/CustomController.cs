using Microsoft.AspNetCore.Mvc;
using Joseco.DDD.Core.Results;

namespace Identity.WebApi.Infrastructure;

public class CustomController : ControllerBase
{
    public IActionResult BuildResult<T>(Result<T> result) =>
        result.IsSuccess
            ? Ok(result.Value)
            : Problem(result);

    private ObjectResult Problem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException();
        }
        int statusCode = ResponseHelper.GetStatusCode(result.Error.Type);
        var probleDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = ResponseHelper.GetTitle(result.Error),
            Detail = ResponseHelper.GetDetail(result.Error),
            Type = ResponseHelper.GetType(result.Error.Type)
        };
        

        return StatusCode(statusCode, probleDetails);
    }

   

}
