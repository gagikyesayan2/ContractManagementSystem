using System.ComponentModel.DataAnnotations;

namespace ContractManagementSystem.Shared.Models.Company.Contract;

public sealed class CreateContractRequestModel
{
    [Required]
    public Guid CompanyId { get; set; }

    [Required]
    public Guid EmployeeAccountId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    [Required]
    public DateOnly EmploymentStartDate { get; set; }

    public DateOnly? EmploymentEndDate { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Wage { get; set; }
}
