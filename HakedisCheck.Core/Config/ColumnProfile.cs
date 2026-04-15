using HakedisCheck.Core.Models;

namespace HakedisCheck.Core.Config;

public sealed class ColumnProfile
{
    public string ProfileName { get; set; } = string.Empty;
    public ExcelFileKind FileKind { get; set; }
    public int HeaderRowIndex { get; set; } = 1;
    public int FirstDataRowIndex { get; set; } = 2;
    public List<string> SelectedSheets { get; set; } = [];
    public Dictionary<LogicalField, string?> ColumnMappings { get; set; } = [];
    public DateTimeOffset SavedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public string? GetMappedHeader(LogicalField field) =>
        ColumnMappings.TryGetValue(field, out var header) && !string.IsNullOrWhiteSpace(header)
            ? header
            : null;

    public bool HasMapping(LogicalField field) => GetMappedHeader(field) is not null;

    public ColumnProfile Clone()
    {
        return new ColumnProfile
        {
            ProfileName = ProfileName,
            FileKind = FileKind,
            HeaderRowIndex = HeaderRowIndex,
            FirstDataRowIndex = FirstDataRowIndex,
            SelectedSheets = [.. SelectedSheets],
            ColumnMappings = ColumnMappings.ToDictionary(pair => pair.Key, pair => pair.Value),
            SavedAtUtc = SavedAtUtc
        };
    }
}
