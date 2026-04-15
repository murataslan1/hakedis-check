using HakedisCheck.Core.Aggregation;
using HakedisCheck.Core.Comparison;
using HakedisCheck.Core.Models;

namespace HakedisCheck.Tests.Comparison;

public sealed class HakedisValidatorTests
{
    [Fact]
    public void Validate_MatchesByNormalizedNameWhenTcIsMissingInMesai()
    {
        var leave = new[]
        {
            new LeaveAggregate
            {
                EmployeeName = "Ayşe Kargılı",
                AnnualLeaveDays = 2m
            }
        };

        var mesai = new[]
        {
            new MesaiAggregate
            {
                EmployeeName = "AYSE KARGILI",
                RegularOvertimeHours = 14.5m,
                OfficialHolidayHours = 0m,
                MealAmount = 1425m
            }
        };

        var hakedis = new[]
        {
            new HakedisEntry("Ayşe Kargılı", "12345678901", 21m, 2m, 14.5m, 0m, 0m, 0m, null, "Mart2026", 3)
        };

        var rows = new HakedisValidator().Validate(leave, mesai, hakedis);

        Assert.Contains(rows, row => row.CheckName == "Yıllık İzin (Gün)" && row.Status == ValidationStatus.Ok);
        Assert.Contains(rows, row => row.CheckName == "Fazla Mesai (Saat)" && row.Status == ValidationStatus.Ok);
    }

    [Fact]
    public void Validate_ReturnsEksikWhenEmployeeIsMissingFromHakedis()
    {
        var leave = new[]
        {
            new LeaveAggregate
            {
                EmployeeName = "Burcu Doğan",
                AnnualLeaveDays = 3m
            }
        };

        var rows = new HakedisValidator().Validate(leave, Array.Empty<MesaiAggregate>(), Array.Empty<HakedisEntry>());

        var row = Assert.Single(rows);
        Assert.Equal(ValidationStatus.Eksik, row.Status);
        Assert.Equal("Hakediş Kaydı", row.CheckName);
    }
}
