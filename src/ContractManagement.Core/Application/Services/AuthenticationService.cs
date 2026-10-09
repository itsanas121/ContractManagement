using ContractManagement.Core.Application.Common;
using Microsoft.AspNetCore.Identity;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Application.DTOs.Authentication;
using Microsoft.EntityFrameworkCore;
using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAppDbContext _dbContext;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthenticationService(
        IAppDbContext dbContext,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
    }


    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Email == request.Email,
            cancellationToken);


        if (user is null)
        {
            throw new InvalidCredentialsException();
            
        }
        if (!user.IsActive)
        {
            throw new DomainException("User account is inactive.");
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            throw new InvalidCredentialsException();
        }


        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse
        {
            Token = token
        };

    }

}
