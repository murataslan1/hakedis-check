namespace HakedisCheck.Core.Models;

public sealed record LeaveEntry(
    string EmployeeName,
    string? IdentityNumber,
    string LeaveType,
    decimal Amount,
    string Unit,
    DateTime? StartDate,
    DateTime? EndDate,
    string SourceSheet,
    int SourceRow);
