using System.ComponentModel.DataAnnotations;

namespace ContractManagementSystem.API.Models.Company.Contract;

public class CreateContractResponseModel
{
    public Guid Id { get; init; }
    [Required]
    public Guid CompanyId { get; init; }
    [Required]
    public Guid EmployeeAccountId { get; init; }
    [Required]
    public string Title { get; init; } = default!;
    public string? Description { get; init; }
    [Required]
    public DateOnly EmploymentStartDate { get; init; }
    public DateOnly? EmploymentEndDate { get; init; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Wage { get; init; }
    public DateTime CreatedAtUtc { get; init; }

    public string? EmployeeFirstName { get; init; }
    public string? EmployeeLastName { get; init; }
}
