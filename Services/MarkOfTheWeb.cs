using System.IO;

namespace FileInfoViewer.Services;

/// <summary>
/// What Windows' "Mark of the Web" says about a file: the zone it came from and, when the program that saved it wrote them,
/// the address it was downloaded from, the page that linked to it and more. <see cref="Extra"/> holds every other
/// key in the stream, so nothing the program wrote is lost.
/// </summary>
public sealed record WebOrigin(int? ZoneId, string? HostUrl, string? ReferrerUrl, IReadOnlyList<KeyValuePair<string, string>> Extra)
{
    /// <summary>The zone number with its Windows name: "3 - Internet".</summary>
    public string? ZoneText => ZoneId switch
    {
        null => null,
        0 => "0 - My Computer",
        1 => "1 - Local Intranet",
        2 => "2 - Trusted Sites",
        3 => "3 - Internet",
        4 => "4 - Restricted Sites",
        var other => other.ToString(),
    };
}

/// <summary>
/// Reads the hidden NTFS stream "Zone.Identifier" that browsers (Chrome, Edge, Brave ...) attach to a downloaded file, an INI text:
///   [ZoneTransfer]
///   ZoneId=3
///   ReferrerUrl=https://page.example/       (the page that linked to the download; not always there)
///   HostUrl=https://files.example/a.jpg     (the address of the file, or only of the site; not always there)
/// The stream is part of the file: it goes when the file is copied to a drive that is not NTFS, when a cloud service strips it,
/// or when the user clicks Unblock. Nothing is written. A file with no stream (the usual case), a drive with no streams, a network
/// share or a folder gives null.
/// Copy of SweWolf File Explorer's Helpers/MarkOfTheWeb.cs. When copying: change the namespace.
/// History:
///   2026-10-24  First version (SweWolf File Explorer only).
///   2026-10-24  FileInfoViewer: keeps every other key of the stream in Extra (HostIpAddress, LastWriterPackageFamilyName ...).
/// </summary>
public static class MarkOfTheWeb
{
    public static WebOrigin? Read(string path)
    {
        try
        {
            using var stream = new FileStream(path + ":Zone.Identifier", FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);

            int? zone = null;
            string? host = null, referrer = null;
            var extra = new List<KeyValuePair<string, string>>();
            bool inSection = false;
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith('[')) { inSection = line.Equals("[ZoneTransfer]", StringComparison.OrdinalIgnoreCase); continue; }
                if (!inSection) continue;

                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                string key = line[..eq].Trim(), value = line[(eq + 1)..].Trim();
                if (value.Length == 0) continue;

                if (key.Equals("ZoneId", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var id)) zone = id;
                else if (key.Equals("HostUrl", StringComparison.OrdinalIgnoreCase)) host = value;
                else if (key.Equals("ReferrerUrl", StringComparison.OrdinalIgnoreCase)) referrer = value;
                else extra.Add(new(key, value));
            }

            return zone == null && host == null && referrer == null && extra.Count == 0 ? null : new WebOrigin(zone, host, referrer, extra);
        }
        catch
        {
            return null;   // no stream, not NTFS, no access, gone: nothing to show
        }
    }
}
