namespace HakedisCheck.Core.Models;

public enum ValidationStatus
{
    Ok,
    Hata,
    Eksik
}

public static class ValidationStatusExtensions
{
    public static string GetDisplayName(this ValidationStatus status) => status switch
    {
        ValidationStatus.Ok => "OK",
        ValidationStatus.Hata => "HATA",
        ValidationStatus.Eksik => "EKSIK",
        _ => status.ToString().ToUpperInvariant()
    };
}
