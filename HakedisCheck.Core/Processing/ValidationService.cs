using HakedisCheck.Core.Aggregation;
using HakedisCheck.Core.Comparison;
using HakedisCheck.Core.Config;
using HakedisCheck.Core.Excel;
using HakedisCheck.Core.Models;
using HakedisCheck.Core.Utilities;

namespace HakedisCheck.Core.Processing;

public sealed class ValidationService
{
    private readonly ClosedXmlReader _reader = new();
    private readonly LeaveAggregator _leaveAggregator = new();
    private readonly MesaiAggregator _mesaiAggregator = new();
    private readonly HakedisValidator _validator = new();

    public WorkbookPreview PreviewWorkbook(string filePath) => _reader.ReadPreview(filePath);

    public ColumnProfile CreateSuggestedProfile(ExcelFileKind fileKind, string filePath, string? profileName = null)
    {
        var preview = PreviewWorkbook(filePath);
        return ColumnProfileFactory.CreateSuggestedProfile(fileKind, preview, profileName);
    }

    public ValidationRunResult Run(ValidationRunOptions options)
    {
        var warnings = new List<SourceWarning>();

        var leaveEntries = ReadLeaveEntries(options.LeaveFilePath, options.LeaveProfile, warnings);
        var mesaiEntries = ReadMesaiEntries(options.MesaiFilePath, options.MesaiProfile, warnings);
        var hakedisEntries = ReadHakedisEntries(options.HakedisFilePath, options.HakedisProfile, warnings);

        var leaveAggregates = _leaveAggregator.Aggregate(leaveEntries);
        var mesaiAggregates = _mesaiAggregator.Aggregate(mesaiEntries);
        var rows = _validator.Validate(leaveAggregates, mesaiAggregates, hakedisEntries);

        return new ValidationRunResult
        {
            Rows = rows,
            Warnings = warnings,
            LeaveEntryCount = leaveEntries.Count,
            MesaiEntryCount = mesaiEntries.Count,
            HakedisEntryCount = hakedisEntries.Count
        };
    }

    private List<LeaveEntry> ReadLeaveEntries(string filePath, ColumnProfile profile, List<SourceWarning> warnings)
    {
        var result = new List<LeaveEntry>();
        foreach (var row in _reader.ReadRows(filePath, profile))
        {
            var employeeName = ReadString(row, profile, LogicalField.EmployeeName);
            if (string.IsNullOrWhiteSpace(employeeName))
            {
                continue;
            }

            result.Add(new LeaveEntry(
                employeeName,
                ReadString(row, profile, LogicalField.IdentityNumber),
                ReadString(row, profile, LogicalField.LeaveType) ?? string.Empty,
                ParseNumeric(row, profile, LogicalField.LeaveAmount, ExcelFileKind.Leave, warnings),
                ReadString(row, profile, LogicalField.LeaveUnit) ?? "gün",
                null,
                null,
                row.SheetName,
                row.RowNumber));
        }

        return result;
    }

    private List<MesaiEntry> ReadMesaiEntries(string filePath, ColumnProfile profile, List<SourceWarning> warnings)
    {
        var result = new List<MesaiEntry>();
        foreach (var row in _reader.ReadRows(filePath, profile))
        {
            var employeeName = ReadString(row, profile, LogicalField.EmployeeName);
            if (string.IsNullOrWhiteSpace(employeeName))
            {
                continue;
            }

            result.Add(new MesaiEntry(
                employeeName,
                ReadString(row, profile, LogicalField.IdentityNumber),
                ParseNumeric(row, profile, LogicalField.WeekdayOvertimeHours, ExcelFileKind.Mesai, warnings, hours: true),
                ParseNumeric(row, profile, LogicalField.WeekendOvertimeHours, ExcelFileKind.Mesai, warnings, hours: true),
                ParseNumeric(row, profile, LogicalField.TotalOvertimeHours, ExcelFileKind.Mesai, warnings, hours: true),
                ParseNumeric(row, profile, LogicalField.OfficialHolidayOvertimeHours, ExcelFileKind.Mesai, warnings, hours: true),
                ParseNumeric(row, profile, LogicalField.TotalOfficialHolidayHours, ExcelFileKind.Mesai, warnings, hours: true),
                ParseNumeric(row, profile, LogicalField.AnnualLeaveDays, ExcelFileKind.Mesai, warnings),
                ParseNumeric(row, profile, LogicalField.ExcuseLeaveDays, ExcelFileKind.Mesai, warnings),
                ParseNumeric(row, profile, LogicalField.AdministrativeLeaveHours, ExcelFileKind.Mesai, warnings, hours: true),
                ParseNumeric(row, profile, LogicalField.MealAmount, ExcelFileKind.Mesai, warnings),
                row.SheetName,
                row.RowNumber));
        }

        return result;
    }

    private List<HakedisEntry> ReadHakedisEntries(string filePath, ColumnProfile profile, List<SourceWarning> warnings)
    {
        var result = new List<HakedisEntry>();
        foreach (var row in _reader.ReadRows(filePath, profile))
        {
            var employeeName = ReadString(row, profile, LogicalField.EmployeeName);
            if (string.IsNullOrWhiteSpace(employeeName))
            {
                continue;
            }

            var mealHeaderMapped = profile.HasMapping(LogicalField.MealAmount);
            result.Add(new HakedisEntry(
                employeeName,
                ReadString(row, profile, LogicalField.IdentityNumber),
                ParseNumeric(row, profile, LogicalField.WorkDays, ExcelFileKind.Hakedis, warnings),
                ParseNumeric(row, profile, LogicalField.UsedAnnualLeaveDays, ExcelFileKind.Hakedis, warnings),
                ParseNumeric(row, profile, LogicalField.OvertimeHours, ExcelFileKind.Hakedis, warnings, hours: true),
                ParseNumeric(row, profile, LogicalField.OvertimeAmount, ExcelFileKind.Hakedis, warnings),
                ParseNumeric(row, profile, LogicalField.HolidayOvertimeHours, ExcelFileKind.Hakedis, warnings, hours: true),
                ParseNumeric(row, profile, LogicalField.HolidayOvertimeAmount, ExcelFileKind.Hakedis, warnings),
                mealHeaderMapped
                    ? ParseNumeric(row, profile, LogicalField.MealAmount, ExcelFileKind.Hakedis, warnings)
                    : null,
                row.SheetName,
                row.RowNumber));
        }

        return result;
    }

    private static string? ReadString(RowData row, ColumnProfile profile, LogicalField field)
    {
        var header = profile.GetMappedHeader(field);
        return header is null ? null : row[header];
    }

    private static decimal ParseNumeric(
        RowData row,
        ColumnProfile profile,
        LogicalField field,
        ExcelFileKind fileKind,
        List<SourceWarning> warnings,
        bool hours = false)
    {
        var rawValue = ReadString(row, profile, field);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return 0m;
        }

        var parsedValue = hours ? ValueParser.ParseHours(rawValue) : ValueParser.ParseDecimal(rawValue);
        if (parsedValue == 0m
            && !string.Equals(rawValue, "0", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(rawValue, "0,0", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(rawValue, "0.0", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(rawValue, "#REF!", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add(new SourceWarning(
                fileKind,
                row.SheetName,
                row.RowNumber,
                $"{ProfileSchema.GetDisplayName(field)} alanı çözümlenemedi: {rawValue}"));
        }

        return parsedValue;
    }
}
