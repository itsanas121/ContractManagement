using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ContractManagement.Api.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // Minimal implementation: do not handle here.
            // Return true if the exception was handled and the pipeline should stop.
            return new ValueTask<bool>(false);
        }
    }
}
