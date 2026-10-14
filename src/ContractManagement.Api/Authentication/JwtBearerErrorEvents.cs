using ContractManagement.Core.Application.Common;
using ContractManagement.Core.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace ContractManagement.Api.Authentication;

public class JwtBearerErrorEvents : JwtBearerEvents
{
    private readonly IAppDbContext _dbContext;
    public JwtBearerErrorEvents(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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

    public override async Task TokenValidated(
        TokenValidatedContext context)
    {

        var userIdClaim = context.Principal?
            .FindFirst("sub")?.Value;

        if (!int.TryParse(userIdClaim, out var userId)
            || userId <= 0)
        {
            context.Fail("Invalid user identity.");
            return;
        }

        // check the user current database status
        var isActive = await _dbContext.Users
            .AsNoTracking().AnyAsync(
            u => u.Id == userId &&
            u.Status == UserStatus.Active,
            context.HttpContext.RequestAborted);

        if (!isActive)
        {
            context.Fail(
                "User account is inactive or dose not exist.");
        }
    }
}

