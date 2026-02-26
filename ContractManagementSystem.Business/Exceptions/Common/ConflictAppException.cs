namespace ContractManagementSystem.Business.Exceptions.Common;

public sealed class ConflictAppException : AppException
{
    public ConflictAppException(string message)
        : base(message, 409)
    {
    }
}

