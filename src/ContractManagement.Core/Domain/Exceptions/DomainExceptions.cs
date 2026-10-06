using ContractManagement.Api.Domain.Enums;

namespace ContractManagement.Api.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InvalidContractStatusException : DomainException
{
    public InvalidContractStatusException(string contractNumber, ContractStatus currentStatus, string action)
        : base($"Cannot {action} contract {contractNumber} because its status is {currentStatus}.") { }
}

public class ContractActivationException : DomainException
{
    public ContractActivationException(string message) : base(message) { }
}

public class DomainValidationException : DomainException
{
    public IReadOnlyList<string> Errors { get; }

    public DomainValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList();
    }
}