using HakedisCheck.Core.Aggregation;
using HakedisCheck.Core.Models;
using HakedisCheck.Core.Utilities;

namespace HakedisCheck.Core.Matching;

public sealed class EmployeeMatcher
{
    public IReadOnlyList<MatchedEmployee> Match(
        IEnumerable<LeaveAggregate> leaveAggregates,
        IEnumerable<MesaiAggregate> mesaiAggregates,
        IEnumerable<HakedisEntry> hakedisEntries)
    {
        var buckets = new List<MatchedEmployee>();
        var byIdentity = new Dictionary<string, MatchedEmployee>(StringComparer.Ordinal);
        var byName = new Dictionary<string, MatchedEmployee>(StringComparer.Ordinal);

        foreach (var leave in leaveAggregates)
        {
            var bucket = GetOrCreateBucket(leave.EmployeeName, leave.IdentityNumber, buckets, byIdentity, byName);
            bucket.Leave = leave;
        }

        foreach (var mesai in mesaiAggregates)
        {
            var bucket = GetOrCreateBucket(mesai.EmployeeName, mesai.IdentityNumber, buckets, byIdentity, byName);
            bucket.Mesai = mesai;
        }

        foreach (var hakedis in hakedisEntries)
        {
            var bucket = GetOrCreateBucket(hakedis.EmployeeName, hakedis.IdentityNumber, buckets, byIdentity, byName);
            bucket.Hakedis = hakedis;
        }

        return buckets
            .OrderBy(bucket => bucket.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public static string BuildAggregateKey(string? identityNumber, string employeeName)
    {
        var normalizedIdentity = ValueParser.NormalizeIdentityNumber(identityNumber);
        return normalizedIdentity is not null
            ? $"TC:{normalizedIdentity}"
            : $"NAME:{NameNormalizer.Normalize(employeeName)}";
    }

    private static MatchedEmployee GetOrCreateBucket(
        string employeeName,
        string? identityNumber,
        List<MatchedEmployee> buckets,
        Dictionary<string, MatchedEmployee> byIdentity,
        Dictionary<string, MatchedEmployee> byName)
    {
        var normalizedIdentity = ValueParser.NormalizeIdentityNumber(identityNumber);
        var normalizedName = NameNormalizer.Normalize(employeeName);

        byIdentity.TryGetValue(normalizedIdentity ?? string.Empty, out var identityBucket);
        byName.TryGetValue(normalizedName, out var nameBucket);

        var bucket = identityBucket ?? nameBucket;
        if (identityBucket is not null && nameBucket is not null && !ReferenceEquals(identityBucket, nameBucket))
        {
            bucket = MergeBuckets(identityBucket, nameBucket, buckets, byIdentity, byName);
        }

        if (bucket is null)
        {
            bucket = new MatchedEmployee
            {
                DisplayName = employeeName,
                NormalizedName = normalizedName,
                IdentityNumber = normalizedIdentity
            };

            buckets.Add(bucket);
        }

        bucket.DisplayName = ChooseDisplayName(bucket.DisplayName, employeeName);
        bucket.NormalizedName = normalizedName.Length > 0 ? normalizedName : bucket.NormalizedName;
        bucket.IdentityNumber ??= normalizedIdentity;

        if (!string.IsNullOrWhiteSpace(normalizedIdentity))
        {
            byIdentity[normalizedIdentity] = bucket;
        }

        if (!string.IsNullOrWhiteSpace(normalizedName))
        {
            byName[normalizedName] = bucket;
        }

        return bucket;
    }

    private static MatchedEmployee MergeBuckets(
        MatchedEmployee preferred,
        MatchedEmployee other,
        List<MatchedEmployee> buckets,
        Dictionary<string, MatchedEmployee> byIdentity,
        Dictionary<string, MatchedEmployee> byName)
    {
        preferred.Leave ??= other.Leave;
        preferred.Mesai ??= other.Mesai;
        preferred.Hakedis ??= other.Hakedis;
        preferred.IdentityNumber ??= other.IdentityNumber;
        preferred.DisplayName = ChooseDisplayName(preferred.DisplayName, other.DisplayName);

        if (!string.IsNullOrWhiteSpace(other.IdentityNumber))
        {
            byIdentity[other.IdentityNumber] = preferred;
        }

        if (!string.IsNullOrWhiteSpace(other.NormalizedName))
        {
            byName[other.NormalizedName] = preferred;
        }

        buckets.Remove(other);
        return preferred;
    }

    private static string ChooseDisplayName(string current, string candidate)
    {
        if (string.IsNullOrWhiteSpace(current))
        {
            return candidate;
        }

        return candidate.Length > current.Length ? candidate : current;
    }
}
