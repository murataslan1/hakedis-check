using HakedisCheck.Core.Config;

namespace HakedisCheck.Core.Processing;

public sealed class ValidationRunOptions
{
    public required string LeaveFilePath { get; init; }
    public required string MesaiFilePath { get; init; }
    public required string HakedisFilePath { get; init; }
    public required ColumnProfile LeaveProfile { get; init; }
    public required ColumnProfile MesaiProfile { get; init; }
    public required ColumnProfile HakedisProfile { get; init; }
}
