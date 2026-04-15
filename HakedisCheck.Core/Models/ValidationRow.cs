namespace HakedisCheck.Core.Models;

public sealed record ValidationRow(
    string EmployeeName,
    string? IdentityNumber,
    string CheckName,
    ValidationStatus Status,
    decimal? ExpectedValue,
    decimal? ActualValue,
    decimal? Difference,
    string Description);
