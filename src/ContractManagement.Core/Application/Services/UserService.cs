using ContractManagement.Core.Application.Common;
using ContractManagement.Core.Application.DTOs.Companies;
using ContractManagement.Core.Application.DTOs.Users;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Core.Application.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(
        IAppDbContext dbContext,
        IAuditService auditService,
        ICurrentUser currentUser,
        IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {

        // validate the password and role
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new DomainException("Password is required.");

        if (request.Role is null ||
            !Enum.IsDefined(request.Role.Value))
            throw new DomainException("A valid user role is required.");

        var role = request.Role.Value;

        // find the company
        var company = await _dbContext.Companies
            .FirstOrDefaultAsync(
            c => c.Id == request.CompanyId,
            cancellationToken);

        if (company is null)
            throw new NotFoundException("Company", request.CompanyId);

        if (!company.IsActive)
            throw new DomainException(
                "Cannot register a user under an inactive company.");


        var firstName = request.FirstName?.Trim() ?? "";
        var lastName = request.LastName?.Trim() ?? "";
        var email = request.Email?.Trim() ?? "";
        var employeeNumber = request.EmployeeNumber?.Trim() ?? "";

        var emailExists = await _dbContext.Users
            .AnyAsync(
            u => u.Email == email,
            cancellationToken);

        if (emailExists)
            throw new DomainException(
                "A user with this email already exists.");


        var employeeNumberExists = await _dbContext.Users
            .AnyAsync(
            u => u.CompanyId == company.Id &&
                u.EmployeeNumber == employeeNumber,
            cancellationToken);

        if (employeeNumberExists)
            throw new DomainException(
                "Employee number already exists in this comapny.");

        // Create temporary user for passwordhashing
        var temporaryUser = new User(
            firstName,
            lastName,
            email,
            employeeNumber,
            "temporary-placeholder",
            role,
            company);

        var passwordHash = _passwordHasher.HashPassword(
            temporaryUser,
            request.Password);

        // Create the actual user with hashed password
        var user = new User(
            firstName,
            lastName,
            email,
            employeeNumber,
            passwordHash,
            role,
            company);

        if (request.Profile is not null)
        {
            user.SetProfile(
                request.Profile.JobTitle,
                request.Profile.Department,
                request.Profile.PhoneNumber);
        }


        // Start DB Transaction
        await using var transaction =
            await _dbContext.BeginTransactionAsync(cancellationToken);

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Record user creation in audit
        _auditService.Log(
            AuditEntityType.User,
            user.Id,
            AuditAction.Created,
            _currentUser.UserId,
            newValue: new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.EmployeeNumber,
                user.Role,
                user.Status,
                user.CompanyId
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);


        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            EmployeeNumber = user.EmployeeNumber,
            Role = user.Role,
            Status = user.Status,
            CompanyId = user.CompanyId,
            CreatedAt = user.CreatedDate,

            Profile = user.Profile is null 
                ? null
                : new UserProfileDto
                {
                    JobTitle = user.Profile.JobTitle,
                    Department = user.Profile.Department,
                    PhoneNumber = user.Profile.PhoneNumber
                }
        };

    }

    public async Task ActivateAsync(
        int userId,
        CancellationToken cancellationToken) 
    {
        // find user and load their company
        var user = await _dbContext.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(
            u => u.Id == userId,
            cancellationToken);

        if (user is null)
            throw new NotFoundException("User", userId);

        if (user.IsActive)
            throw new DomainException("User is already active.");

        // Save previous status for audit logging
        var oldStatus = user.Status;

        // Activate the user
        user.Activate();

        // Record status change
        _auditService.Log(
            AuditEntityType.User,
            user.Id,
            AuditAction.Activated,
            _currentUser.UserId,
            oldValue: new { Status = oldStatus },
            newValue: new { Status = user.Status });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        // find user and load their company
        var user = await _dbContext.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(
            u => u.Id == userId,
            cancellationToken);

        if (user is null)
            throw new NotFoundException("User", userId);

        if (!user.IsActive)
            throw new DomainException("User is already inactive.");

        // Save previous status for audit logging
        var oldStatus = user.Status;

        // Activate the user
        user.Deactivate();

        // Record status change
        _auditService.Log(
            AuditEntityType.User,
            user.Id,
            AuditAction.Deactivated,
            _currentUser.UserId,
            oldValue: new { Status = oldStatus },
            newValue: new { Status = user.Status });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<UserResponse>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedDate)
            .ThenBy(u => u.Id)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                EmployeeNumber = u.EmployeeNumber,
                Role = u.Role,
                Status = u.Status,
                CompanyId = u.CompanyId,
                CreatedAt = u.CreatedDate,

                Profile = u.Profile == null
                ? null
                : new UserProfileDto
                {
                    JobTitle = u.Profile.JobTitle,
                    Department = u.Profile.Department,
                    PhoneNumber = u.Profile.PhoneNumber
                }
            })
        .ToListAsync(cancellationToken);
    }
}
