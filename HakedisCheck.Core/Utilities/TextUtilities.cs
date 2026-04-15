using System.Text;

namespace HakedisCheck.Core.Utilities;

public static class TextUtilities
{
    public static string CollapseWhitespace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        var previousWasWhitespace = false;

        foreach (var character in value.Trim())
        {
            if (char.IsWhiteSpace(character))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                    previousWasWhitespace = true;
                }

                continue;
            }

            builder.Append(character);
            previousWasWhitespace = false;
        }

        return builder.ToString();
    }

    public static string NormalizeForLookup(string? value)
    {
        var collapsed = CollapseWhitespace(value);
        if (collapsed.Length == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(collapsed.Length);
        foreach (var character in collapsed.ToUpperInvariant())
        {
            var mapped = character switch
            {
                'İ' or 'I' or 'ı' => 'I',
                'Ş' => 'S',
                'Ğ' => 'G',
                'Ü' => 'U',
                'Ö' => 'O',
                'Ç' => 'C',
                _ => character
            };

            if (char.IsLetterOrDigit(mapped) || char.IsWhiteSpace(mapped))
            {
                builder.Append(mapped);
            }
        }

        return CollapseWhitespace(builder.ToString());
    }
}
