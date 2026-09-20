using FileInfoViewer.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Exceptions;

namespace FileInfoViewer.Services;

internal static class PdfInfoReader
{
    public static PdfInfoModel Read(string filePath)
    {
        var model = new PdfInfoModel();
        try
        {
            using var doc = PdfDocument.Open(filePath);
            model.PageCount  = doc.NumberOfPages;
            model.PdfVersion = $"PDF {doc.Version:0.0}";

            var info = doc.Information;
            model.Title    = info.Title    ?? "";
            model.Author   = info.Author   ?? "";
            model.Subject  = info.Subject  ?? "";
            model.Keywords = info.Keywords ?? "";
            model.Creator  = info.Creator  ?? "";
            model.Producer = info.Producer ?? "";

            if (TryParsePdfDate(info.CreationDate, out var created))  model.CreationDate = created;
            if (TryParsePdfDate(info.ModifiedDate,  out var modified)) model.ModifiedDate = modified;
        }
        catch (PdfDocumentEncryptedException)
        {
            model.IsEncrypted = true;
        }
        return model;
    }

    // PDF date format: D:YYYYMMDDHHmmSS[OHH'mm']  — timezone offset is ignored, result is UTC
    private static bool TryParsePdfDate(string? raw, out DateTime result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var s = raw.Trim();
        if (s.StartsWith("D:", StringComparison.OrdinalIgnoreCase)) s = s[2..];
        if (s.Length < 4) return false;

        var year  = s[..4];
        var month = s.Length >= 6  ? s[4..6]   : "01";
        var day   = s.Length >= 8  ? s[6..8]   : "01";
        var hour  = s.Length >= 10 ? s[8..10]  : "00";
        var min   = s.Length >= 12 ? s[10..12] : "00";
        var sec   = s.Length >= 14 ? s[12..14] : "00";

        if (!DateTime.TryParse($"{year}-{month}-{day}T{hour}:{min}:{sec}", out result)) return false;
        result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
        return true;
    }
}
