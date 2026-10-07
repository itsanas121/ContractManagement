using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Api.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred.");

            var statusCode = exception switch {
                DomainValidationException => StatusCodes.Status400BadRequest,
                InvalidContractStatusException => StatusCodes.Status409Conflict,
                ContractActivationException => StatusCodes.Status409Conflict,
                DomainException => StatusCodes.Status400BadRequest,

                _ => StatusCodes.Status500InternalServerError
            };

            ProblemDetails problemDetails = new ProblemDetails()
            {
                Title = statusCode == 500 ? "Internal Server Error" : "Business Rule Violation",
                Status = statusCode,
                Detail = exception is DomainException ? exception.Message : "An unexpected error occurred."
            };

            if (exception is DomainValidationException validationException)
            {
                problemDetails.Extensions["errors"] =
                    validationException.Errors;
            }

            httpContext.Response.StatusCode =
                problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            // Minimal implementation: do not handle here.
            // Return true if the exception was handled and the pipeline should stop.
            //return new ValueTask<bool>(false);
            return true;
        }
    }
}
