using ContractManagement.Core.Domain.Enums;

namespace ContractManagement.Core.Domain.Exceptions;

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
public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with ID '{id}' was not found.")
    {
    }
}
public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Invalid email or password.")
    {
    }
}