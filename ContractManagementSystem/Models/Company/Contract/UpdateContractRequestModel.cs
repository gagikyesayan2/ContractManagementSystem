using System.ComponentModel.DataAnnotations;

namespace ContractManagementSystem.API.Models.Company.Contract;

public sealed class UpdateContractRequestModel
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = default!;

    public string? Description { get; init; }

    [Required]
    public DateOnly EmploymentStartDate { get; init; }

    [Required]
    public DateOnly? EmploymentEndDate { get; init; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Wage { get; init; }
}
