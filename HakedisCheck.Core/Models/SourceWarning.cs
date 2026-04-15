namespace HakedisCheck.Core.Models;

public sealed record SourceWarning(
    ExcelFileKind FileKind,
    string SheetName,
    int RowNumber,
    string Message);
