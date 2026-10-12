using ContractManagement.Core.Application.Common;

namespace ContractManagement.Core.Tests.Fakes;

public sealed class FakeCurrentUser : ICurrentUser
{
    public int? UserId { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
}