using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using ContractManagement.Core.Application.Common;
using ContractManagement.Core.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace ContractManagement.Infrastructure.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly IClock _clock;

        public JwtTokenGenerator(IConfiguration configuration, IClock clock)
        {
            _configuration = configuration;
            _clock = clock;
        }

        public string GenerateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim('role', user.Role)
            };

        }


    }
}
