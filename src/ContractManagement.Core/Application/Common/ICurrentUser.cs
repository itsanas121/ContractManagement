using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.Common
{
    public interface ICurrentUser
    {
        int? UserId { get; }
        bool IsAuthenticated { get; }
    }
}
