namespace HakedisCheck.Core.Excel;

public sealed class RowData
{
    public required string SheetName { get; init; }
    public required int RowNumber { get; init; }
    public Dictionary<string, string?> Values { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public string? this[string header] => Values.TryGetValue(header, out var value) ? value : null;
}
