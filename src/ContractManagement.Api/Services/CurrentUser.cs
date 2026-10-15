using ContractManagement.Core.Application.Common;
using System.Security.Claims;

namespace ContractManagement.Api.Services;

    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId {
        
            get {
                var userIdClaim = _httpContextAccessor
                    .HttpContext?
                    .User?
                    .FindFirst("sub")?.Value; // "sub" is the claim type for user ID in JWT tokens"

                return int.TryParse(userIdClaim, out int userId)
                    ? userId
                    : null;
            }
        }
        
        public bool IsAuthenticated {

            get
            {
                return _httpContextAccessor
                    .HttpContext?
                    .User?
                    .Identity?
                    .IsAuthenticated ?? false;
            }
        
        } 
        
        


    }

