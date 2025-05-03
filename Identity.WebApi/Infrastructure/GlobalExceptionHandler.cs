using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Joseco.DDD.Core.Results;

namespace Identity.WebApi.Infrastructure;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        if(exception is DomainException domainException)
        {
            var error = domainException.Error;
            var domainProblemDetails = new ProblemDetails
            {
                Status = ResponseHelper.GetStatusCode(error.Type),
                Title = ResponseHelper.GetTitle(error),
                Detail = ResponseHelper.GetDetail(error),
                Type = ResponseHelper.GetType(error.Type),
            };
            httpContext.Response.StatusCode = domainProblemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(domainProblemDetails, cancellationToken);
            return true;
        }

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = "Server failure"
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
