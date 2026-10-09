using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.Common
{
    public interface IClock
    {
        DateTime UtcNow { get; }

    }
}
