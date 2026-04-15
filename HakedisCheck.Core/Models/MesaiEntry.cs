namespace HakedisCheck.Core.Models;

public sealed record MesaiEntry(
    string EmployeeName,
    string? IdentityNumber,
    decimal WeekdayOvertimeHours,
    decimal WeekendOvertimeHours,
    decimal TotalOvertimeHours,
    decimal OfficialHolidayHours,
    decimal TotalOfficialHolidayHours,
    decimal AnnualLeaveDays,
    decimal ExcuseLeaveDays,
    decimal AdministrativeLeaveHours,
    decimal MealAmount,
    string SourceSheet,
    int SourceRow);
