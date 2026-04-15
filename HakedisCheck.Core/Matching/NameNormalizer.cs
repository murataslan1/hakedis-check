using HakedisCheck.Core.Utilities;

namespace HakedisCheck.Core.Matching;

public static class NameNormalizer
{
    public static string Normalize(string? value) => TextUtilities.NormalizeForLookup(value);
}
