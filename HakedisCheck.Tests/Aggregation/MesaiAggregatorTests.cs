using HakedisCheck.Core.Aggregation;
using HakedisCheck.Core.Models;

namespace HakedisCheck.Tests.Aggregation;

public sealed class MesaiAggregatorTests
{
    [Fact]
    public void Aggregate_PrefersTotalColumnsAndFallsBackToWeeklyColumns()
    {
        var entries = new[]
        {
            new MesaiEntry("Aykut Gerden", "11980040474", 0m, 0m, 21.25m, 8m, 8m, 0m, 0m, 0m, 1425m, "Bilgi Sis.", 3),
            new MesaiEntry("Aykut Gerden", "11980040474", 1m, 2m, 0m, 4m, 0m, 0m, 0m, 0m, 100m, "Bilgi Sis.", 33)
        };

        var aggregates = new MesaiAggregator().Aggregate(entries);

        var aggregate = Assert.Single(aggregates);
        Assert.Equal(24.25m, aggregate.RegularOvertimeHours);
        Assert.Equal(12m, aggregate.OfficialHolidayHours);
        Assert.Equal(1525m, aggregate.MealAmount);
    }
}
