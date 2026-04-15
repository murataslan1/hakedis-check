using HakedisCheck.Core.Matching;
using HakedisCheck.Core.Models;
using HakedisCheck.Core.Utilities;

namespace HakedisCheck.Core.Aggregation;

public sealed class MesaiAggregator
{
    public IReadOnlyList<MesaiAggregate> Aggregate(IEnumerable<MesaiEntry> entries)
    {
        var aggregates = new Dictionary<string, MesaiAggregate>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            var key = EmployeeMatcher.BuildAggregateKey(entry.IdentityNumber, entry.EmployeeName);
            if (!aggregates.TryGetValue(key, out var aggregate))
            {
                aggregate = new MesaiAggregate
                {
                    EmployeeName = entry.EmployeeName,
                    IdentityNumber = ValueParser.NormalizeIdentityNumber(entry.IdentityNumber)
                };

                aggregates[key] = aggregate;
            }

            aggregate.RegularOvertimeHours += entry.TotalOvertimeHours != 0
                ? entry.TotalOvertimeHours
                : entry.WeekdayOvertimeHours + entry.WeekendOvertimeHours;

            aggregate.OfficialHolidayHours += entry.TotalOfficialHolidayHours != 0
                ? entry.TotalOfficialHolidayHours
                : entry.OfficialHolidayHours;

            aggregate.MealAmount += entry.MealAmount;
            aggregate.AnnualLeaveDays += entry.AnnualLeaveDays;
            aggregate.ExcuseLeaveDays += entry.ExcuseLeaveDays;
            aggregate.AdministrativeLeaveHours += entry.AdministrativeLeaveHours;
        }

        return aggregates.Values.OrderBy(item => item.EmployeeName, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }
}
