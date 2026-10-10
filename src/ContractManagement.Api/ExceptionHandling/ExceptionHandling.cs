using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Api.ExceptionHandling;

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
            if (exception is DomainException)
            {
                _logger.LogWarning(
                    exception,
                    "A business rule violation occurred.");
            } else
            {
                _logger.LogError(
                     exception,
                     "An unhandled exception occurred.");
            }
     

            var statusCode = exception switch {
                DomainValidationException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                InvalidContractStatusException => StatusCodes.Status409Conflict,
                ContractActivationException => StatusCodes.Status409Conflict,
                InvalidCredentialsException => StatusCodes.Status401Unauthorized,
                DomainException => StatusCodes.Status400BadRequest,

                _ => StatusCodes.Status500InternalServerError
            };

            var title = exception switch
            {
                NotFoundException => "Not Found",
                InvalidCredentialsException => "Unauthorized",
                DomainException => "Business Rule Violation",

                _ => "Internal Server Error"
            };

            var detail = exception switch
            {
                DomainException => exception.Message,

                _ => "An unexpected error occurred."
            };

            ProblemDetails problemDetails = new ProblemDetails()
            {
                Title = title,
                Status = statusCode,
                Detail = detail
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

            return true;
        }
    }

