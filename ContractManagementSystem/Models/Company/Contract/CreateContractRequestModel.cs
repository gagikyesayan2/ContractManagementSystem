using System.ComponentModel.DataAnnotations;

namespace ContractManagementSystem.API.Models.Company.Contract;

public sealed class CreateContractRequestModel
{
    [Required]
    public Guid CompanyId { get; init; }

    [Required]
    public Guid EmployeeAccountId { get; init; }

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = default!;

    public string? Description { get; init; }

    [Required]
    public DateOnly EmploymentStartDate { get; init; }

    public DateOnly? EmploymentEndDate { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal Wage { get; init; }
}
