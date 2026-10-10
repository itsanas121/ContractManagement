using ContractManagement.Core.Application.Common;
using ContractManagement.Core.Application.Services;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Infrastructure.Persistence;
using ContractManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ContractManagement.Infrastructure.Auditing;

namespace ContractManagement.Infrastructure;

public static class DependencyInjection
{
    // One method for Program.cs to call, so the Api doesn't need to know what is inside Infrastructure
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        // when a service asks for IAppDbContext, it gets the same AppDbContext instance
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // when a service asks for IClock, it gets the same SystemClock instance
        services.AddSingleton<IClock, SystemClock>();

        // when a service asks for IJwtTokenGenerator, it gets the same JwtTokenGenerator instance
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddScoped<IAuditService, AuditService>();
        
        return services;
    }
}