namespace HakedisCheck.Core.Excel;

public sealed record WorksheetPreview(string Name, IReadOnlyList<PreviewRow> Rows)
{
    public IReadOnlyList<string> GetHeaders(int rowNumber) =>
        Rows.FirstOrDefault(row => row.RowNumber == rowNumber)?.Cells ?? Array.Empty<string>();

    public string ToMultilinePreview()
    {
        return string.Join(
            Environment.NewLine,
            Rows.Select(row => $"R{row.RowNumber}: {string.Join(" | ", row.Cells)}"));
    }
}
