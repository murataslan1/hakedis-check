namespace HakedisCheck.Core.Aggregation;

public sealed class MesaiAggregate
{
    public required string EmployeeName { get; init; }
    public string? IdentityNumber { get; init; }
    public decimal RegularOvertimeHours { get; set; }
    public decimal OfficialHolidayHours { get; set; }
    public decimal MealAmount { get; set; }
    public decimal AnnualLeaveDays { get; set; }
    public decimal ExcuseLeaveDays { get; set; }
    public decimal AdministrativeLeaveHours { get; set; }
}
