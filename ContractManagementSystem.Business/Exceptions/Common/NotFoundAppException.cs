namespace ContractManagementSystem.Business.Exceptions.Common;


public sealed class NotFoundAppException : AppException
{
    public NotFoundAppException(string message)
        : base(message, 404)
    {
    }
}
