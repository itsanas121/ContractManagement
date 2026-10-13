using ContractManagement.Core.Application.Common;
using ContractManagement.Core.Application.DTOs.Companies;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IAppDbContext _dbContext;
        private readonly IAuditService _auditService;
        private readonly ICurrentUser _currentUser;

        public CompanyService(
            IAppDbContext dbContext,
            IAuditService auditService,
            ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _auditService = auditService;
            _currentUser = currentUser;
        }

        public async Task<CompanyResponse> CreateAsync(
            CreateCompanyRequest request,
            CancellationToken cancellationToken = default)
        {
            var exists = await _dbContext.Companies.AnyAsync(
                c => c.RegistrationNumber == request.RegistrationNumber,
                cancellationToken);

            if (exists)
            {
                throw new DomainException("A company with the same registration number already exists.");
            }

            var company = new Company(
                request.Name,
                request.RegistrationNumber,
                request.Email,
                request.Phone,
                request.Address);

            // start transaction
            await using var transaction =
                await _dbContext.BeginTransactionAsync(cancellationToken);

            _dbContext.Companies.Add(company);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _auditService.Log(
                AuditEntityType.Company,
                company.Id,
                AuditAction.Created,
                _currentUser.UserId,
                newValue: new
                {
                    company.Name,
                    company.RegistrationNumber,
                    company.Status
                });

            // save changes and commit transaction
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new CompanyResponse
            {
                Id = company.Id,
                Name = company.Name,
                RegistrationNumber = company.RegistrationNumber,
                Email = company.Email,
                Phone = company.Phone,
                Address = company.Address,
                Status = company.Status,
                CreatedAt = company.CreatedDate
            };
        }
        
        public async Task<List<CompanyResponse>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Companies
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CompanyResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    RegistrationNumber = c.RegistrationNumber,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address,
                    Status = c.Status,
                    CreatedAt = c.CreatedDate
                })
                .ToListAsync(cancellationToken);
        }
    }
}
