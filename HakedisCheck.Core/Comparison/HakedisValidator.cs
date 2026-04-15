using HakedisCheck.Core.Aggregation;
using HakedisCheck.Core.Matching;
using HakedisCheck.Core.Models;

namespace HakedisCheck.Core.Comparison;

public sealed class HakedisValidator
{
    private readonly EmployeeMatcher _employeeMatcher = new();

    public IReadOnlyList<ValidationRow> Validate(
        IEnumerable<LeaveAggregate> leaveAggregates,
        IEnumerable<MesaiAggregate> mesaiAggregates,
        IEnumerable<HakedisEntry> hakedisEntries)
    {
        var rows = new List<ValidationRow>();
        var employees = _employeeMatcher.Match(leaveAggregates, mesaiAggregates, hakedisEntries);

        foreach (var employee in employees)
        {
            if (employee.Hakedis is null)
            {
                rows.Add(new ValidationRow(
                    employee.DisplayName,
                    employee.IdentityNumber,
                    "Hakediş Kaydı",
                    ValidationStatus.Eksik,
                    1,
                    0,
                    -1,
                    "Personel kaynak dosyalarda bulundu ancak hakediş sayfasında bulunamadı."));
                continue;
            }

            if (employee.Mesai is null
                && (employee.Hakedis.OvertimeHours != 0
                    || employee.Hakedis.HolidayOvertimeHours != 0
                    || employee.Hakedis.MealAmount.GetValueOrDefault() != 0))
            {
                rows.Add(new ValidationRow(
                    employee.DisplayName,
                    employee.IdentityNumber,
                    "Mesai Kaydı",
                    ValidationStatus.Eksik,
                    1,
                    0,
                    -1,
                    "Hakedişte mesai verisi var fakat mesai/puantaj dosyasında personel bulunamadı."));
            }

            if (employee.Leave is null && employee.Hakedis.UsedAnnualLeaveDays != 0)
            {
                rows.Add(new ValidationRow(
                    employee.DisplayName,
                    employee.IdentityNumber,
                    "İzin Kaydı",
                    ValidationStatus.Eksik,
                    1,
                    0,
                    -1,
                    "Hakedişte yıllık izin var fakat izin dosyasında personel bulunamadı."));
            }

            rows.Add(CreateComparisonRow(
                employee.DisplayName,
                employee.IdentityNumber,
                "Yıllık İzin (Gün)",
                employee.Leave?.AnnualLeaveDays ?? 0,
                employee.Hakedis.UsedAnnualLeaveDays));

            rows.Add(CreateComparisonRow(
                employee.DisplayName,
                employee.IdentityNumber,
                "Fazla Mesai (Saat)",
                employee.Mesai?.RegularOvertimeHours ?? 0,
                employee.Hakedis.OvertimeHours));

            rows.Add(CreateComparisonRow(
                employee.DisplayName,
                employee.IdentityNumber,
                "Resmi Tatil FM (Saat)",
                employee.Mesai?.OfficialHolidayHours ?? 0,
                employee.Hakedis.HolidayOvertimeHours));

            if (employee.Hakedis.MealAmount is decimal mealAmount)
            {
                rows.Add(CreateComparisonRow(
                    employee.DisplayName,
                    employee.IdentityNumber,
                    "Yemek Tutarı (TL)",
                    employee.Mesai?.MealAmount ?? 0,
                    mealAmount));
            }
            else if (employee.Mesai is not null)
            {
                rows.Add(new ValidationRow(
                    employee.DisplayName,
                    employee.IdentityNumber,
                    "Yemek Tutarı (TL)",
                    ValidationStatus.Ok,
                    employee.Mesai.MealAmount,
                    null,
                    null,
                    "Hakediş kolon eşlemesinde yemek alanı seçilmediği için bilgi amaçlı gösterildi."));
            }
        }

        return rows;
    }

    private static ValidationRow CreateComparisonRow(
        string employeeName,
        string? identityNumber,
        string checkName,
        decimal expectedValue,
        decimal actualValue)
    {
        expectedValue = decimal.Round(expectedValue, 4);
        actualValue = decimal.Round(actualValue, 4);
        var difference = decimal.Round(actualValue - expectedValue, 4);
        var status = difference == 0 ? ValidationStatus.Ok : ValidationStatus.Hata;

        return new ValidationRow(
            employeeName,
            identityNumber,
            checkName,
            status,
            expectedValue,
            actualValue,
            difference,
            status == ValidationStatus.Ok
                ? "Beklenen değer ile hakediş değeri eşleşiyor."
                : "Beklenen değer ile hakediş değeri farklı.");
    }
}
