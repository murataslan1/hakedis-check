using ClosedXML.Excel;
using HakedisCheck.Core.Config;
using HakedisCheck.Core.Utilities;

namespace HakedisCheck.Core.Excel;

public sealed class ClosedXmlReader
{
    public WorkbookPreview ReadPreview(string filePath, int sampleRowCount = 8, int maxColumns = 40)
    {
        using var workbook = new XLWorkbook(filePath);
        var worksheets = workbook.Worksheets
            .Select(worksheet => ReadWorksheetPreview(worksheet, sampleRowCount, maxColumns))
            .ToArray();

        return new WorkbookPreview(filePath, worksheets);
    }

    public IReadOnlyList<RowData> ReadRows(string filePath, ColumnProfile profile)
    {
        using var workbook = new XLWorkbook(filePath);
        var selectedSheets = profile.SelectedSheets.Count > 0
            ? profile.SelectedSheets
            : workbook.Worksheets.Select(worksheet => worksheet.Name).ToList();

        var rows = new List<RowData>();

        foreach (var sheetName in selectedSheets)
        {
            var worksheet = workbook.Worksheets.FirstOrDefault(item =>
                string.Equals(item.Name, sheetName, StringComparison.OrdinalIgnoreCase));

            if (worksheet is null)
            {
                continue;
            }

            rows.AddRange(ReadWorksheetRows(worksheet, profile));
        }

        return rows;
    }

    private static WorksheetPreview ReadWorksheetPreview(IXLWorksheet worksheet, int sampleRowCount, int maxColumns)
    {
        var lastRow = Math.Max(worksheet.LastRowUsed()?.RowNumber() ?? sampleRowCount, sampleRowCount);
        var lastColumn = Math.Min(worksheet.LastColumnUsed()?.ColumnNumber() ?? maxColumns, maxColumns);
        var rows = new List<PreviewRow>();

        for (var rowIndex = 1; rowIndex <= Math.Min(sampleRowCount, lastRow); rowIndex++)
        {
            var cells = new List<string>();
            for (var columnIndex = 1; columnIndex <= lastColumn; columnIndex++)
            {
                cells.Add(GetCellText(worksheet.Cell(rowIndex, columnIndex)));
            }

            rows.Add(new PreviewRow(rowIndex, cells));
        }

        return new WorksheetPreview(worksheet.Name, rows);
    }

    private static IEnumerable<RowData> ReadWorksheetRows(IXLWorksheet worksheet, ColumnProfile profile)
    {
        var headerMap = ReadHeaderMap(worksheet, profile.HeaderRowIndex);
        if (headerMap.Count == 0)
        {
            yield break;
        }

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? profile.FirstDataRowIndex;
        for (var rowIndex = profile.FirstDataRowIndex; rowIndex <= lastRow; rowIndex++)
        {
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var (header, columnIndex) in headerMap)
            {
                values[header] = GetCellText(worksheet.Cell(rowIndex, columnIndex));
            }

            if (values.Values.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            yield return new RowData
            {
                SheetName = worksheet.Name,
                RowNumber = rowIndex,
                Values = values
            };
        }
    }

    private static Dictionary<string, int> ReadHeaderMap(IXLWorksheet worksheet, int headerRowIndex)
    {
        var lastColumn = worksheet.Row(headerRowIndex).LastCellUsed()?.Address.ColumnNumber
            ?? worksheet.LastColumnUsed()?.ColumnNumber()
            ?? 0;

        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var columnIndex = 1; columnIndex <= lastColumn; columnIndex++)
        {
            var header = GetCellText(worksheet.Cell(headerRowIndex, columnIndex));
            if (string.IsNullOrWhiteSpace(header) || result.ContainsKey(header))
            {
                continue;
            }

            result[header] = columnIndex;
        }

        return result;
    }

    private static string GetCellText(IXLCell cell)
    {
        try
        {
            var formatted = cell.GetFormattedString();
            if (!string.IsNullOrWhiteSpace(formatted))
            {
                return TextUtilities.CollapseWhitespace(formatted);
            }
        }
        catch
        {
            // Fall back to raw string value if formatting fails.
        }

        return TextUtilities.CollapseWhitespace(cell.Value.ToString());
    }
}
