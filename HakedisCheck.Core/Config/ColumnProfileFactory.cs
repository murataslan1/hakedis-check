using HakedisCheck.Core.Excel;
using HakedisCheck.Core.Models;
using HakedisCheck.Core.Utilities;

namespace HakedisCheck.Core.Config;

public static class ColumnProfileFactory
{
    public static ColumnProfile CreateSuggestedProfile(
        ExcelFileKind kind,
        WorkbookPreview preview,
        string? profileName = null)
    {
        var selectedSheets = SelectDefaultSheets(kind, preview).ToList();
        var referenceSheet = selectedSheets
            .Select(preview.FindWorksheet)
            .FirstOrDefault(worksheet => worksheet is not null)
            ?? preview.Worksheets.FirstOrDefault();

        if (referenceSheet is null)
        {
            return new ColumnProfile
            {
                FileKind = kind,
                ProfileName = profileName ?? $"{kind.GetDisplayName()} Otomatik"
            };
        }

        var fields = ProfileSchema.GetFields(kind);
        var headerRowIndex = FindBestHeaderRow(referenceSheet, fields);
        var headerCells = referenceSheet.GetHeaders(headerRowIndex);
        var mappings = BuildMappings(headerCells, fields);
        var firstDataRowIndex = FindFirstDataRow(referenceSheet, headerRowIndex, mappings);

        return new ColumnProfile
        {
            FileKind = kind,
            ProfileName = profileName ?? $"{kind.GetDisplayName()} Otomatik",
            HeaderRowIndex = headerRowIndex,
            FirstDataRowIndex = firstDataRowIndex,
            SelectedSheets = selectedSheets,
            ColumnMappings = mappings
        };
    }

    private static IEnumerable<string> SelectDefaultSheets(ExcelFileKind kind, WorkbookPreview preview)
    {
        return kind switch
        {
            ExcelFileKind.Leave => preview.WorksheetNames.Take(1),
            ExcelFileKind.Mesai => preview.WorksheetNames,
            ExcelFileKind.Hakedis => preview.WorksheetNames
                .Where(name => !TextUtilities.NormalizeForLookup(name).Contains("BIRIM FIYATLAR", StringComparison.Ordinal))
                .TakeLast(1),
            _ => preview.WorksheetNames.Take(1)
        };
    }

    private static int FindBestHeaderRow(WorksheetPreview worksheet, IReadOnlyList<LogicalField> fields)
    {
        var aliasLookup = fields
            .ToDictionary(
                field => field,
                field => ProfileSchema
                    .GetAliases(field)
                    .Select(TextUtilities.NormalizeForLookup)
                    .ToArray());

        var bestRow = worksheet.Rows.FirstOrDefault()?.RowNumber ?? 1;
        var bestScore = -1;

        foreach (var row in worksheet.Rows)
        {
            var score = row.Cells
                .Select(TextUtilities.NormalizeForLookup)
                .Count(cell => aliasLookup.Values.Any(aliases => aliases.Any(alias => cell.Contains(alias, StringComparison.Ordinal))));

            if (score > bestScore)
            {
                bestScore = score;
                bestRow = row.RowNumber;
            }
        }

        return bestRow;
    }

    private static Dictionary<LogicalField, string?> BuildMappings(
        IReadOnlyList<string> headers,
        IReadOnlyList<LogicalField> fields)
    {
        var mappings = new Dictionary<LogicalField, string?>();
        foreach (var field in fields)
        {
            var aliases = ProfileSchema.GetAliases(field)
                .Select(TextUtilities.NormalizeForLookup)
                .ToArray();

            var match = headers
                .Where(header => !string.IsNullOrWhiteSpace(header))
                .Select(header => new
                {
                    Header = header,
                    Normalized = TextUtilities.NormalizeForLookup(header)
                })
                .FirstOrDefault(header =>
                    aliases.Any(alias =>
                        header.Normalized.Equals(alias, StringComparison.Ordinal)
                        || header.Normalized.Contains(alias, StringComparison.Ordinal)
                        || alias.Contains(header.Normalized, StringComparison.Ordinal)));

            mappings[field] = match?.Header;
        }

        return mappings;
    }

    private static int FindFirstDataRow(
        WorksheetPreview worksheet,
        int headerRowIndex,
        IReadOnlyDictionary<LogicalField, string?> mappings)
    {
        var nameHeader = mappings.TryGetValue(LogicalField.EmployeeName, out var mappedNameHeader)
            ? mappedNameHeader
            : null;

        if (string.IsNullOrWhiteSpace(nameHeader))
        {
            return headerRowIndex + 1;
        }

        var headers = worksheet.GetHeaders(headerRowIndex);
        var nameColumnIndex = headers
            .Select((header, index) => new { Header = header, Index = index })
            .FirstOrDefault(item => string.Equals(item.Header, nameHeader, StringComparison.OrdinalIgnoreCase))
            ?.Index ?? 0;

        foreach (var row in worksheet.Rows.Where(row => row.RowNumber > headerRowIndex))
        {
            if (row.Cells.Count > nameColumnIndex && !string.IsNullOrWhiteSpace(row.Cells[nameColumnIndex]))
            {
                return row.RowNumber;
            }
        }

        return headerRowIndex + 1;
    }
}
