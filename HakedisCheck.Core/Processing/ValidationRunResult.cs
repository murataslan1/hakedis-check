using HakedisCheck.Core.Models;

namespace HakedisCheck.Core.Processing;

public sealed class ValidationRunResult
{
    public required IReadOnlyList<ValidationRow> Rows { get; init; }
    public required IReadOnlyList<SourceWarning> Warnings { get; init; }
    public required int LeaveEntryCount { get; init; }
    public required int MesaiEntryCount { get; init; }
    public required int HakedisEntryCount { get; init; }

    public int ErrorCount => Rows.Count(row => row.Status == ValidationStatus.Hata);
    public int MissingCount => Rows.Count(row => row.Status == ValidationStatus.Eksik);
    public int OkCount => Rows.Count(row => row.Status == ValidationStatus.Ok);
}
