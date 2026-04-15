namespace HakedisCheck.Core.Aggregation;

public sealed class LeaveAggregate
{
    public required string EmployeeName { get; init; }
    public string? IdentityNumber { get; init; }
    public decimal AnnualLeaveDays { get; set; }
    public decimal ExcuseLeaveDays { get; set; }
    public decimal OtherLeaveDays { get; set; }
}
