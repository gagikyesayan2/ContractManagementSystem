namespace ContractManagementSystem.Business.Exceptions.Common;

public sealed class ValidationAppException : AppException
{
    public ValidationAppException(string message)
        : base(message, 400)
    {
    }
}
