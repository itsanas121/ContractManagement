using ContractManagement.Core.Application.Common;

namespace ContractManagement.Infrastructure.Services;

internal class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}
