using System;
using System.Collections.Generic;
using System.Text;

using ContractManagement.Core.Domain.Entities;

namespace ContractManagement.Core.Application.Common;

    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }

