namespace ContractManagementSystem.Shared.Models.Company;


public sealed class CreateCompanyResponseModel
{
    public string Name { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
    public Guid CompanyId { get; set; }
}
