namespace ContractManagementSystem.API.Models.Company;

public sealed class CreateCompanyResponseModel
{
    public string Name { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
}
