using HakedisCheck.Core.Matching;
using HakedisCheck.Core.Models;
using HakedisCheck.Core.Utilities;

namespace HakedisCheck.Core.Aggregation;

public sealed class LeaveAggregator
{
    public IReadOnlyList<LeaveAggregate> Aggregate(IEnumerable<LeaveEntry> entries)
    {
        var aggregates = new Dictionary<string, LeaveAggregate>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            var key = EmployeeMatcher.BuildAggregateKey(entry.IdentityNumber, entry.EmployeeName);
            if (!aggregates.TryGetValue(key, out var aggregate))
            {
                aggregate = new LeaveAggregate
                {
                    EmployeeName = entry.EmployeeName,
                    IdentityNumber = ValueParser.NormalizeIdentityNumber(entry.IdentityNumber)
                };

                aggregates[key] = aggregate;
            }

            var normalizedType = NameNormalizer.Normalize(entry.LeaveType);
            if (normalizedType.Contains("YILLIK IZIN", StringComparison.Ordinal))
            {
                aggregate.AnnualLeaveDays += entry.Amount;
            }
            else if (normalizedType.Contains("MAZERET", StringComparison.Ordinal))
            {
                aggregate.ExcuseLeaveDays += entry.Amount;
            }
            else
            {
                aggregate.OtherLeaveDays += entry.Amount;
            }
        }

        return aggregates.Values.OrderBy(item => item.EmployeeName, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }
}
