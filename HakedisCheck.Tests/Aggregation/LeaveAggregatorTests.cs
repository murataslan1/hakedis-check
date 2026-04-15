using HakedisCheck.Core.Aggregation;
using HakedisCheck.Core.Models;

namespace HakedisCheck.Tests.Aggregation;

public sealed class LeaveAggregatorTests
{
    [Fact]
    public void Aggregate_SumsAnnualAndExcuseLeaveByNormalizedEmployee()
    {
        var entries = new[]
        {
            new LeaveEntry("Ayşe Kargılı", null, "yıllık izin", 3m, "gün", null, null, "Sayfa1", 3),
            new LeaveEntry("AYSE   KARGILI", null, "yıllık izin (yarım gün)", 0.5m, "gün", null, null, "Sayfa1", 4),
            new LeaveEntry("Ayşe Kargılı", null, "mazeret izni", 1m, "gün", null, null, "Sayfa1", 5)
        };

        var aggregates = new LeaveAggregator().Aggregate(entries);

        var aggregate = Assert.Single(aggregates);
        Assert.Equal(3.5m, aggregate.AnnualLeaveDays);
        Assert.Equal(1m, aggregate.ExcuseLeaveDays);
    }
}
