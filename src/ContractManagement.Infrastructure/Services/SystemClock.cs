using System;
using System.Collections.Generic;
using System.Text;

using ContractManagement.Core.Application.Common;

namespace ContractManagement.Infrastructure.Services
{
    internal class SystemClock : IClock
    {
        public DateTime UtcNow {

            get {
                return DateTime.UtcNow;

            } 
        
        }
    }
}
