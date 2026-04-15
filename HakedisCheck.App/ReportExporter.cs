using ClosedXML.Excel;
using HakedisCheck.Core.Models;
using HakedisCheck.Core.Processing;

namespace HakedisCheck.App;

public sealed class ReportExporter
{
    public void Export(ValidationRunResult result, string outputPath)
    {
        using var workbook = new XLWorkbook();
        var resultSheet = workbook.Worksheets.Add("Sonuclar");
        var headers = new[]
        {
            "Personel",
            "T.C.",
            "Kontrol",
            "Durum",
            "Beklenen",
            "Bulunan",
            "Fark",
            "Açıklama"
        };

        for (var index = 0; index < headers.Length; index++)
        {
            resultSheet.Cell(1, index + 1).Value = headers[index];
            resultSheet.Cell(1, index + 1).Style.Font.Bold = true;
        }

        for (var rowIndex = 0; rowIndex < result.Rows.Count; rowIndex++)
        {
            var row = result.Rows[rowIndex];
            var excelRow = rowIndex + 2;
            resultSheet.Cell(excelRow, 1).Value = row.EmployeeName;
            resultSheet.Cell(excelRow, 2).Value = row.IdentityNumber;
            resultSheet.Cell(excelRow, 3).Value = row.CheckName;
            resultSheet.Cell(excelRow, 4).Value = row.Status.GetDisplayName();
            resultSheet.Cell(excelRow, 5).Value = row.ExpectedValue;
            resultSheet.Cell(excelRow, 6).Value = row.ActualValue;
            resultSheet.Cell(excelRow, 7).Value = row.Difference;
            resultSheet.Cell(excelRow, 8).Value = row.Description;

            var fillColor = row.Status switch
            {
                ValidationStatus.Ok => XLColor.LightGreen,
                ValidationStatus.Hata => XLColor.LightPink,
                ValidationStatus.Eksik => XLColor.LightYellow,
                _ => XLColor.NoColor
            };

            resultSheet.Range(excelRow, 1, excelRow, headers.Length).Style.Fill.BackgroundColor = fillColor;
        }

        resultSheet.Columns().AdjustToContents();

        if (result.Warnings.Count > 0)
        {
            var warningSheet = workbook.Worksheets.Add("Uyarilar");
            warningSheet.Cell(1, 1).Value = "Dosya";
            warningSheet.Cell(1, 2).Value = "Sayfa";
            warningSheet.Cell(1, 3).Value = "Satır";
            warningSheet.Cell(1, 4).Value = "Mesaj";
            warningSheet.Range(1, 1, 1, 4).Style.Font.Bold = true;

            for (var rowIndex = 0; rowIndex < result.Warnings.Count; rowIndex++)
            {
                var warning = result.Warnings[rowIndex];
                var excelRow = rowIndex + 2;
                warningSheet.Cell(excelRow, 1).Value = warning.FileKind.GetDisplayName();
                warningSheet.Cell(excelRow, 2).Value = warning.SheetName;
                warningSheet.Cell(excelRow, 3).Value = warning.RowNumber;
                warningSheet.Cell(excelRow, 4).Value = warning.Message;
            }

            warningSheet.Columns().AdjustToContents();
        }

        workbook.SaveAs(outputPath);
    }
}
