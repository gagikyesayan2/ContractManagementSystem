namespace ContractManagementSystem.Business.Enum
{
    public enum ContractStatusFilter
    {
        All = 0,
        NotStarted = 1, // StartDate > today
        Active = 2,     // StartDate <= today && (EndDate is null || EndDate >= today)
        Finished = 3    // EndDate < today
    }
}
