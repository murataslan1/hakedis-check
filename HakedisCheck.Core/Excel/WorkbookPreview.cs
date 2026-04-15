namespace HakedisCheck.Core.Excel;

public sealed record WorkbookPreview(string FilePath, IReadOnlyList<WorksheetPreview> Worksheets)
{
    public IReadOnlyList<string> WorksheetNames => Worksheets.Select(worksheet => worksheet.Name).ToArray();

    public WorksheetPreview? FindWorksheet(string name) =>
        Worksheets.FirstOrDefault(worksheet => string.Equals(worksheet.Name, name, StringComparison.OrdinalIgnoreCase));
}
