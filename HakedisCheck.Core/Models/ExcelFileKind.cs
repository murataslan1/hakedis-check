namespace HakedisCheck.Core.Models;

public enum ExcelFileKind
{
    Leave,
    Mesai,
    Hakedis
}

public static class ExcelFileKindExtensions
{
    public static string GetDisplayName(this ExcelFileKind kind) => kind switch
    {
        ExcelFileKind.Leave => "İzin",
        ExcelFileKind.Mesai => "Mesai",
        ExcelFileKind.Hakedis => "Hakediş",
        _ => kind.ToString()
    };
}
