using ContractManagementSystem.Data.Entities.Common;
using ContractManagementSystem.Data.Enums;

namespace ContractManagementSystem.Data.Entities;


public class Account : BaseEntity
{

    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

}
