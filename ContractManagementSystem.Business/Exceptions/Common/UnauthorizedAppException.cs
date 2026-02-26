namespace ContractManagementSystem.Business.Exceptions.Common;

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message)
        : base(message, 401)
    {
    }
}
