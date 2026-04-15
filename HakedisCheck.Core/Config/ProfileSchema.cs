using HakedisCheck.Core.Models;

namespace HakedisCheck.Core.Config;

public static class ProfileSchema
{
    private static readonly IReadOnlyDictionary<LogicalField, string> DisplayNames =
        new Dictionary<LogicalField, string>
        {
            [LogicalField.EmployeeName] = "Ad Soyad",
            [LogicalField.IdentityNumber] = "T.C. Kimlik No",
            [LogicalField.LeaveType] = "İzin Türü",
            [LogicalField.LeaveAmount] = "İzin Miktarı",
            [LogicalField.LeaveUnit] = "İzin Birimi",
            [LogicalField.WeekdayOvertimeHours] = "Hafta İçi Mesai Saati",
            [LogicalField.WeekendOvertimeHours] = "Hafta Sonu Mesai Saati",
            [LogicalField.TotalOvertimeHours] = "Toplam Mesai Saati",
            [LogicalField.OfficialHolidayOvertimeHours] = "Resmi Tatil Mesai Saati",
            [LogicalField.TotalOfficialHolidayHours] = "Toplam Resmi Tatil Mesai Saati",
            [LogicalField.MealAmount] = "Yemek Tutarı",
            [LogicalField.AnnualLeaveDays] = "Yıllık İzin Gün",
            [LogicalField.ExcuseLeaveDays] = "Mazeret İzni",
            [LogicalField.AdministrativeLeaveHours] = "İdari İzin",
            [LogicalField.WorkDays] = "Hakedişe Esas İş Günü",
            [LogicalField.UsedAnnualLeaveDays] = "Kullanılan Yıllık İzin",
            [LogicalField.OvertimeHours] = "Fazla Mesai Saat",
            [LogicalField.OvertimeAmount] = "Fazla Mesai Ücreti",
            [LogicalField.HolidayOvertimeHours] = "Resmi Tatil FM Saat",
            [LogicalField.HolidayOvertimeAmount] = "Resmi Tatil FM Ücret"
        };

    private static readonly IReadOnlyDictionary<LogicalField, string[]> Aliases =
        new Dictionary<LogicalField, string[]>
        {
            [LogicalField.EmployeeName] = ["AD SOYAD", "ADI SOYADI", "DANIŞMAN ADI SOYADI", "DANISMAN ADI SOYADI"],
            [LogicalField.IdentityNumber] = ["T.C.", "T.C. KIMLIK NO", "T.C. KİMLİK NO"],
            [LogicalField.LeaveType] = ["İZİN", "IZIN"],
            [LogicalField.LeaveAmount] = ["GÜN", "GUN", "MİKTAR", "MIKTAR"],
            [LogicalField.LeaveUnit] = ["BİRİM", "BIRIM"],
            [LogicalField.WeekdayOvertimeHours] = ["HAFTA İÇİ", "HAFTA ICI"],
            [LogicalField.WeekendOvertimeHours] = ["HAFTA SONU"],
            [LogicalField.TotalOvertimeHours] = ["TOPLAM HAFTA İÇİ-SONU MESAİ SAATİ", "TOPLAM HAFTA ICI-SONU MESAI SAATI", "TOPLAM MESAİ", "TOPLAM MESAI", "FAZLA MESAİ"],
            [LogicalField.OfficialHolidayOvertimeHours] = ["RESMİ TATİL MESAİ", "RESMI TATIL MESAI"],
            [LogicalField.TotalOfficialHolidayHours] = ["TOPLAM RESMİ TATİL MESAİ SAATİ", "TOPLAM RESMI TATIL MESAI SAATI", "TOLAM RESMI TATIL"],
            [LogicalField.MealAmount] = ["TOPLAM YEMEK TUTARI TL", "YEMEK ÜCRETİ", "YEMEK UCRETI", "YEMEK YATIRILACAK TUTAR"],
            [LogicalField.AnnualLeaveDays] = ["YILLIK İZİN", "YILLIK IZIN", "AYLIK KULLANILAN YILLIK İZİN (GÜN)", "AYLIK KULLANILAN YILLIK IZIN (GUN)"],
            [LogicalField.ExcuseLeaveDays] = ["MAZERET İZNİ", "MAZERET IZNI"],
            [LogicalField.AdministrativeLeaveHours] = ["İDARİ İZİN", "IDARI IZIN", "TOPLAM İDARİ İZİN MESAİ SAATİ", "TOPLAM IDARI IZIN MESAI SAATI"],
            [LogicalField.WorkDays] = ["HAKEDİŞE ESAS İŞ GÜNÜ", "HAKEDISE ESAS IS GUNU"],
            [LogicalField.UsedAnnualLeaveDays] = ["AYLIK KULLANILAN YILLIK İZİN (GÜN)", "AYLIK KULLANILAN YILLIK IZIN (GUN)"],
            [LogicalField.OvertimeHours] = ["FAZLA MESAİ", "FAZLA MESAI", "3 SAAT VE ÜZERİ FAZLA MESAİ", "3 SAAT VE UZERI FAZLA MESAI"],
            [LogicalField.OvertimeAmount] = ["FAZLA MESAİ ÜCRETİ", "FAZLA MESAI UCRETI"],
            [LogicalField.HolidayOvertimeHours] = ["RESMİ TATİL FM SAAT", "RESMI TATIL FM SAAT"],
            [LogicalField.HolidayOvertimeAmount] = ["RESMİ TATİL FM ÜCRET", "RESMI TATIL FM UCRET"]
        };

    public static IReadOnlyList<LogicalField> GetFields(ExcelFileKind kind) => kind switch
    {
        ExcelFileKind.Leave =>
        [
            LogicalField.EmployeeName,
            LogicalField.IdentityNumber,
            LogicalField.LeaveType,
            LogicalField.LeaveAmount,
            LogicalField.LeaveUnit
        ],
        ExcelFileKind.Mesai =>
        [
            LogicalField.EmployeeName,
            LogicalField.IdentityNumber,
            LogicalField.WeekdayOvertimeHours,
            LogicalField.WeekendOvertimeHours,
            LogicalField.TotalOvertimeHours,
            LogicalField.OfficialHolidayOvertimeHours,
            LogicalField.TotalOfficialHolidayHours,
            LogicalField.MealAmount,
            LogicalField.AnnualLeaveDays,
            LogicalField.ExcuseLeaveDays,
            LogicalField.AdministrativeLeaveHours
        ],
        ExcelFileKind.Hakedis =>
        [
            LogicalField.EmployeeName,
            LogicalField.IdentityNumber,
            LogicalField.WorkDays,
            LogicalField.UsedAnnualLeaveDays,
            LogicalField.OvertimeHours,
            LogicalField.OvertimeAmount,
            LogicalField.HolidayOvertimeHours,
            LogicalField.HolidayOvertimeAmount,
            LogicalField.MealAmount
        ],
        _ => Array.Empty<LogicalField>()
    };

    public static string GetDisplayName(LogicalField field) => DisplayNames[field];

    public static IReadOnlyList<string> GetAliases(LogicalField field) => Aliases[field];
}
