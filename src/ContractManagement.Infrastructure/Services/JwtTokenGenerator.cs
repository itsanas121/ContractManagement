using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using ContractManagement.Core.Application.Common;
using ContractManagement.Core.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

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
                new Claim("role", user.Role.ToString())
            };

            // jwtkey -> encoded -> bytes -> SymmtricSecurityKey -> SigningCredentials -> SHA256
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key is not configured.");

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
                );

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
                );

            // token information
            var issur = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT issuer is not configured.");
            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT audience is not configured.");
            var expiryMinutes = int.Parse(
                _configuration["Jwt:ExpiryMinutes"] ?? "60"
                );

            var token = new JwtSecurityToken(
                issuer: issur,
                audience: audience,
                claims: claims,
                notBefore: _clock.UtcNow,
                expires: _clock.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
                );

            //jwt string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
