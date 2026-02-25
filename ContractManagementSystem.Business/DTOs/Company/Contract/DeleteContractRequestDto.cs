namespace ContractManagementSystem.Business.DTOs.Company.Contract;

public sealed class DeleteContractRequestDto
{
    public Guid ContractId { get; init; }
    public Guid CompanyId { get; init; }
    public Guid DeletedByAdminAccountId { get; init; }
}
