using System.Globalization;
using System.Text.RegularExpressions;

namespace HakedisCheck.Core.Utilities;

public static partial class ValueParser
{
    public static decimal ParseDecimal(string? value)
    {
        var sanitized = TextUtilities.CollapseWhitespace(value);
        if (string.IsNullOrWhiteSpace(sanitized) || sanitized == "#REF!" || sanitized == "-")
        {
            return 0m;
        }

        sanitized = sanitized.Replace("TL", string.Empty, StringComparison.OrdinalIgnoreCase);
        sanitized = sanitized.Replace("%", string.Empty, StringComparison.OrdinalIgnoreCase);

        if (decimal.TryParse(sanitized, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out var trValue))
        {
            return trValue;
        }

        if (decimal.TryParse(sanitized, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantValue))
        {
            return invariantValue;
        }

        sanitized = sanitized.Replace(".", string.Empty).Replace(",", ".", StringComparison.Ordinal);
        if (decimal.TryParse(sanitized, NumberStyles.Any, CultureInfo.InvariantCulture, out invariantValue))
        {
            return invariantValue;
        }

        return 0m;
    }

    public static decimal ParseHours(string? value)
    {
        var sanitized = TextUtilities.CollapseWhitespace(value);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return 0m;
        }

        if (TimeSpan.TryParse(sanitized, CultureInfo.InvariantCulture, out var span))
        {
            return (decimal)span.TotalHours;
        }

        var dayMatch = DayTimeRegex().Match(sanitized);
        if (dayMatch.Success)
        {
            var dayCount = int.Parse(dayMatch.Groups["days"].Value, CultureInfo.InvariantCulture);
            if (TimeSpan.TryParse(dayMatch.Groups["time"].Value, CultureInfo.InvariantCulture, out span))
            {
                return (decimal)(TimeSpan.FromDays(dayCount) + span).TotalHours;
            }
        }

        return ParseDecimal(sanitized);
    }

    public static DateTime? ParseDate(string? value)
    {
        var sanitized = TextUtilities.CollapseWhitespace(value);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return null;
        }

        if (DateTime.TryParse(sanitized, CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out var date))
        {
            return date;
        }

        if (DateTime.TryParse(sanitized, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return date;
        }

        return null;
    }

    public static string? NormalizeIdentityNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length == 11 ? digits : null;
    }

    [GeneratedRegex("^(?<days>\\d+)\\s+day[s]?,\\s+(?<time>\\d{1,2}:\\d{2}:\\d{2})$", RegexOptions.IgnoreCase)]
    private static partial Regex DayTimeRegex();
}
