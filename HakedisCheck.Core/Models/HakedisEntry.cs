namespace HakedisCheck.Core.Models;

public sealed record HakedisEntry(
    string EmployeeName,
    string? IdentityNumber,
    decimal WorkDays,
    decimal UsedAnnualLeaveDays,
    decimal OvertimeHours,
    decimal OvertimeAmount,
    decimal HolidayOvertimeHours,
    decimal HolidayOvertimeAmount,
    decimal? MealAmount,
    string SourceSheet,
    int SourceRow);
