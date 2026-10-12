using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Api.Authentication;

public class JwtBearerErrorEvents : JwtBearerEvents
{
    public override async Task Challenge(
        JwtBearerChallengeContext context)
    {
        context.HandleResponse();
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        context.Response.Headers.WWWAuthenticate = "Bearer";

        await context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized",
                Detail = "Authentication is required or the token is invalid."
            });
    }
    public override async Task Forbidden(
        ForbiddenContext context)
    {
        context.Response.StatusCode =
                    StatusCodes.Status403Forbidden;

        await context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = 403,
                Title = "Forbidden",
                Detail = "You do not have permission to perform this action."
            });
    }
}

