using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web;
using FileInfoViewer.Models;
using FileInfoViewer.Services;

namespace FileInfoViewer.Services;

public static class HtmlReportGenerator
{
    public static string Generate(FileInfoModel model)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"FileInfoViewer_{Path.GetRandomFileName()}.html");
        var html = BuildHtml(model);
        File.WriteAllText(tempFile, html, Encoding.UTF8);
        return tempFile;
    }

    private static string BuildHtml(FileInfoModel model)
    {
        var sb = new StringBuilder();
        var icon = GetFileIcon(model.Extension);
        var localCreated = model.CreatedUtc.ToLocalTime();
        var localModified = model.ModifiedUtc.ToLocalTime();
        var localAccessed = model.AccessedUtc.ToLocalTime();
        var settings     = SettingsService.Current;
        var dtf          = CultureInfo.CurrentCulture.DateTimeFormat;
        var tsFmt        = dtf.ShortDatePattern + " " + (settings.ShowSeconds ? dtf.LongTimePattern : dtf.ShortTimePattern);
        var copyDisplay  = settings.CopyButtonDisplay; // "No", "Yes", "Yes on hover over"
        var showOwner    = settings.ShowOwner;
        var showAttribs  = settings.ShowFileAttributes;
        var showHashes   = settings.ShowFileHashes;
        var tzDisplay = settings.TimeZoneDisplay;

        sb.AppendLine($$"""
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<meta name="generator" content="FileInfoViewer v{{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0"}} — {{DateTime.Now:yyyy-MM-dd HH:mm:ss}}">
<title>File Info: {{H(model.FileName)}}</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: 'Segoe UI', system-ui, sans-serif; background: #f0f2f5; color: #1a1a2e; line-height: 1.6; }
  .header { background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
            color: #fff; padding: 2rem 2.5rem; display: flex; align-items: center; gap: 1.5rem; }
  .header .icon { font-size: 3rem; }
  .header h1 { font-size: 1.8rem; font-weight: 600; word-break: break-all; }
  .header .subtitle { font-size: 0.9rem; opacity: 0.75; margin-top: 0.2rem; word-break: break-all; }
  .content { max-width: 1100px; margin: 2rem auto; padding: 0 1.5rem 3rem; }
  .card { background: #fff; border-radius: 12px; box-shadow: 0 2px 12px rgba(0,0,0,.07);
          margin-bottom: 1.5rem; overflow: hidden; }
  .card-header { background: #f8f9fc; border-bottom: 1px solid #e8eaf0; padding: 0.9rem 1.4rem;
                 font-weight: 600; font-size: 0.95rem; color: #444; display: flex; align-items: center; gap: 0.5rem; }
  .card-header .badge { background: #0f3460; color: #fff; border-radius: 999px;
                        font-size: 0.75rem; padding: 0.1rem 0.6rem; }
  table { width: 100%; border-collapse: collapse; }
  td { padding: 0.65rem 1.4rem; font-size: 0.9rem; border-bottom: 1px solid #f0f2f5; vertical-align: top; }
  td:first-child { width: 200px; color: #666; font-weight: 500; white-space: nowrap; }
  td:last-child { color: #222; word-break: break-all; }
  tr:last-child td { border-bottom: none; }
  tr:hover td { background: #fafbff; }
  .hash { font-family: 'Consolas', 'Courier New', monospace; font-size: 0.82rem; color: #2c5f8a;
          word-break: break-all; }
  .attr-chip { display: inline-block; background: #e8f0fe; color: #1a73e8; border-radius: 999px;
               font-size: 0.78rem; padding: 0.15rem 0.7rem; margin: 0.15rem; font-weight: 500; }
  .warn-chip { background: #fff3cd; color: #856404; }
  .section-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; }
  @media (max-width: 700px) { .section-grid { grid-template-columns: 1fr; } td:first-child { width: auto; } }
  .tag-table td:first-child { width: 280px; font-family: 'Consolas', monospace; font-size: 0.82rem; }
  .size-big { font-size: 1.2rem; font-weight: 700; color: #0f3460; }
  .copy-btn { background: none; border: 1px solid #ddd; border-radius: 4px; cursor: pointer;
              font-size: .75rem; padding: .1rem .35rem; margin-left: .5rem; color: #aaa;
              vertical-align: middle; transition: opacity .15s, color .15s, background .15s; }
  .copy-btn:hover { background: #e8f0fe; color: #1a73e8; border-color: #aac4f0; }
  .copy-hover { opacity: 0; }
  tr:hover .copy-hover { opacity: 1; }
  .hover-parent:hover .copy-hover { opacity: 1; }
</style>
<script>
function copyFileName(btn) {
  navigator.clipboard.writeText(btn.dataset.copy).then(() => {
    var orig = btn.textContent;
    btn.textContent = '✓';
    btn.style.color = '#0a8a50';
    setTimeout(function() { btn.textContent = orig; btn.style.color = ''; }, 1500);
  });
}
</script>
</head>
<body>
<div class="header">
  <div class="icon">{{icon}}</div>
  <div>
    <h1>{{H(model.FileName)}}</h1>
    <div class="subtitle">{{H(model.FullPath)}}</div>
  </div>
</div>
<div class="content">
""");

        // Basic info + timestamps side by side
        sb.AppendLine("""<div class="section-grid">""");

        // Basic info card
        sb.AppendLine("""
  <div class="card">
    <div class="card-header">📄 Basic Information</div>
    <table>
""");
        Row(sb, "File Name", H(model.FileName) + CopyBtn(model.FileName, copyDisplay), raw: true);
        Row(sb, "Extension", string.IsNullOrEmpty(model.Extension) ? "(none)" : model.Extension);
        Row(sb, "Directory", H(model.DirectoryPath) + CopyBtn(model.DirectoryPath, copyDisplay), raw: true);
        Row(sb, "Size", $"""<span class="size-big">{H(model.SizeFormatted)}</span> &nbsp;<span style="color:#999;font-size:.85rem;">({model.SizeBytes:N0} bytes)</span>""", raw: true);
        Row(sb, "MIME Type", model.MimeType);
        if (showOwner) Row(sb, "Owner", model.Owner);
        sb.AppendLine("    </table></div>");

        // Timestamps card
        sb.AppendLine("""
  <div class="card">
    <div class="card-header">🕐 Timestamps</div>
    <table>
""");
        if (tzDisplay == "Local" || tzDisplay == "Both")
        {
            Row(sb, "Created",  localCreated.ToString(tsFmt));
            Row(sb, "Modified", localModified.ToString(tsFmt));
            Row(sb, "Accessed", localAccessed.ToString(tsFmt));
        }
        if (tzDisplay == "UTC" || tzDisplay == "Both")
        {
            Row(sb, "Created (UTC)",  model.CreatedUtc.ToString(tsFmt));
            Row(sb, "Modified (UTC)", model.ModifiedUtc.ToString(tsFmt));
            Row(sb, "Accessed (UTC)", model.AccessedUtc.ToString(tsFmt));
        }
        sb.AppendLine("    </table></div>");

        sb.AppendLine("</div>"); // end section-grid

        // Attributes card
        if (showAttribs && !string.IsNullOrEmpty(model.FileAttributes))
        {
            sb.AppendLine("""<div class="card"><div class="card-header">🏷️ File Attributes</div><div style="padding:1rem 1.4rem">""");
            foreach (var attr in model.FileAttributes.Split(',', StringSplitOptions.TrimEntries))
                sb.Append($"""<span class="attr-chip">{H(attr)}</span>""");
            sb.AppendLine("</div></div>");
        }

        // Hashes card
        if (showHashes && (!string.IsNullOrEmpty(model.Md5) || !string.IsNullOrEmpty(model.Sha256)))
        {
            sb.AppendLine("""
<div class="card">
  <div class="card-header">🔑 File Hashes</div>
  <table>
""");
            if (!string.IsNullOrEmpty(model.Md5))
                Row(sb, "MD5", $"""<span class="hash">{model.Md5}</span>""" + CopyBtn(model.Md5, copyDisplay), raw: true);
            if (!string.IsNullOrEmpty(model.Sha256))
                Row(sb, "SHA-256", $"""<span class="hash">{model.Sha256}</span>""" + CopyBtn(model.Sha256, copyDisplay), raw: true);
            sb.AppendLine("  </table></div>");
        }

        // Version info card
        if (model.VersionInfo is { } vi)
        {
            sb.AppendLine("""
<div class="card">
  <div class="card-header">⚙️ Version Information</div>
  <table>
""");
            RowIfSet(sb, "Product Name", vi.ProductName);
            RowIfSet(sb, "File Version", vi.FileVersion);
            RowIfSet(sb, "Product Version", vi.ProductVersion);
            RowIfSet(sb, "Company", vi.CompanyName);
            RowIfSet(sb, "Description", vi.FileDescription);
            RowIfSet(sb, "Copyright", vi.Copyright);
            RowIfSet(sb, "Original Filename", vi.OriginalFilename);
            RowIfSet(sb, "Internal Name", vi.InternalName);
            RowIfSet(sb, "Language", vi.Language);
            Row(sb, "Is Debug", vi.IsDebug ? "Yes" : "No");
            Row(sb, "Is Pre-Release", vi.IsPreRelease ? "Yes" : "No");
            Row(sb, "Is Patched", vi.IsPatched ? "Yes" : "No");
            sb.AppendLine("  </table></div>");
        }

        // Assembly info card
        if (model.AssemblyInfo is { } ai)
        {
            sb.AppendLine("""
<div class="card">
  <div class="card-header">🔧 .NET Assembly Information</div>
  <table>
""");
            RowIfSet(sb, "Assembly Version", ai.AssemblyVersion);
            RowIfSet(sb, "Target Framework", ai.TargetFramework);
            RowIfSet(sb, "Architecture", ai.Architecture);
            Row(sb, "Is Managed", ai.IsManaged ? "Yes" : "No");
            if (ai.ReferencedAssemblies.Count > 0)
                Row(sb, "Referenced Assemblies", string.Join(", ", ai.ReferencedAssemblies.Select(H)));
            sb.AppendLine("  </table></div>");
        }

        // Image info card
        if (model.ImageInfo is { } img)
        {
            sb.AppendLine("""
<div class="card">
  <div class="card-header">🖼️ Image Information</div>
  <table>
""");
            Row(sb, "Dimensions", $"{img.Width} × {img.Height} pixels");
            Row(sb, "DPI", $"{img.HorizontalDpi:F1} × {img.VerticalDpi:F1}");
            Row(sb, "Pixel Format", img.PixelFormat);
            Row(sb, "Bit Depth", img.BitDepth.ToString());
            Row(sb, "Megapixels", $"{img.Width * (long)img.Height / 1_000_000.0:F2} MP");
            sb.AppendLine("  </table>");

            if (img.ExifTags.Count > 0)
            {
                sb.AppendLine($"""  <div class="card-header" style="border-top:1px solid #e8eaf0">📷 EXIF / Metadata</div><table class="tag-table">""");
                foreach (var (key, value) in img.ExifTags.OrderBy(x => x.Key))
                    Row(sb, key, value);
                sb.AppendLine("  </table>");
            }
            sb.AppendLine("</div>");
        }

        // Text info card
        if (model.TextInfo is { } ti)
        {
            sb.AppendLine("""
<div class="card">
  <div class="card-header">📝 Text File Statistics</div>
  <table>
""");
            Row(sb, "Lines", $"{ti.LineCount:N0}");
            Row(sb, "Words", $"{ti.WordCount:N0}");
            Row(sb, "Characters", $"{ti.CharCount:N0}");
            Row(sb, "Encoding", ti.DetectedEncoding);
            Row(sb, "BOM Present", ti.HasBom ? "Yes" : "No");
            sb.AppendLine("  </table></div>");
        }

        // Audio info card
        if (model.AudioInfo is { } aud)
        {
            sb.AppendLine("""
<div class="card">
  <div class="card-header">🎵 Audio Information</div>
  <table>
""");
            RowIfSetCopy(sb, "Title",   aud.Title,   copyDisplay);
            RowIfSetCopy(sb, "Artist",  aud.Artist,  copyDisplay);
            RowIfSet(sb, "Album Artist", aud.AlbumArtist);
            RowIfSet(sb, "Album", aud.Album);
            RowIfSet(sb, "Year", aud.Year);
            RowIfSet(sb, "Track Number", aud.TrackNumber);
            RowIfSet(sb, "Genre", aud.Genre);
            RowIfSet(sb, "Composer", aud.Composer);
            RowIfSetCopy(sb, "Comment", aud.Comment, copyDisplay);
            if (!string.IsNullOrWhiteSpace(aud.AudioSourceUrl))
            {
                var isWebLink = aud.AudioSourceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                             || aud.AudioSourceUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
                if (isWebLink)
                    Row(sb, "Audio Source", $"""<a href="{H(aud.AudioSourceUrl)}" target="_blank" rel="noopener noreferrer">{H(aud.AudioSourceUrl)}</a>""" + CopyBtn(aud.AudioSourceUrl, copyDisplay), raw: true);
                else
                    Row(sb, "Audio Source", H(aud.AudioSourceUrl) + CopyBtn(aud.AudioSourceUrl, copyDisplay), raw: true);
            }
            RowIfSet(sb, "Duration", aud.Duration);
            RowIfSet(sb, "Bit Rate", aud.BitRate);
            RowIfSet(sb, "Sample Rate", aud.SampleRate);
            RowIfSet(sb, "Channels", aud.Channels);
            RowIfSet(sb, "Bit Depth", aud.BitDepth);
            sb.AppendLine("  </table>");

            if (aud.HasCoverArt && aud.CoverArtBase64 != null)
            {
                var altText = string.IsNullOrWhiteSpace(aud.CoverArtDescription) ? "Cover Art" : H(aud.CoverArtDescription);
                sb.AppendLine($$"""
  <div class="card-header" style="border-top:1px solid #e8eaf0">🖼️ Cover Art</div>
  <div style="padding:1.2rem 1.4rem;display:flex;align-items:flex-start;gap:1.4rem">
    <img src="data:{{aud.CoverArtMimeType}};base64,{{aud.CoverArtBase64}}"
         alt="{{altText}}"
         style="max-width:220px;max-height:220px;border-radius:8px;box-shadow:0 2px 10px rgba(0,0,0,.18);object-fit:contain;flex-shrink:0" />
    <table style="border-collapse:collapse;font-size:.88rem;align-self:center">
      <tr><td style="color:#666;font-weight:500;padding:0.3rem 1rem 0.3rem 0;white-space:nowrap">Picture MIME Type</td><td style="color:#222">{{H(aud.CoverArtMimeType)}}</td></tr>
      <tr><td style="color:#666;font-weight:500;padding:0.3rem 1rem 0.3rem 0;white-space:nowrap">Picture Type</td><td style="color:#222">{{H(aud.CoverArtPictureType)}}</td></tr>
      <tr><td style="color:#666;font-weight:500;padding:0.3rem 1rem 0.3rem 0;white-space:nowrap">Picture Description</td><td style="color:#222">{{H(aud.CoverArtDescription)}}</td></tr>
    </table>
  </div>
""");
            }

            if (!string.IsNullOrWhiteSpace(aud.Lyrics))
            {
                sb.AppendLine("""  <div class="hover-parent">""");
                sb.AppendLine($"""    <div class="card-header" style="border-top:1px solid #e8eaf0">🎤 Lyrics{CopyBtn(aud.Lyrics, copyDisplay)}</div>""");
                sb.AppendLine($"""    <div style="padding:1rem 1.4rem;white-space:pre-wrap;font-size:.9rem;line-height:1.8;color:#333">{H(aud.Lyrics)}</div>""");
                sb.AppendLine("""  </div>""");
            }

            if (aud.AllTags.Count > 0)
            {
                sb.AppendLine($"""  <div class="card-header" style="border-top:1px solid #e8eaf0">🏷️ All Metadata Tags</div><table class="tag-table">""");
                foreach (var (key, value) in aud.AllTags.OrderBy(x => x.Key))
                    Row(sb, key, value);
                sb.AppendLine("  </table>");
            }

            sb.AppendLine("</div>");
        }

        // Warnings
        if (model.Warnings.Count > 0)
        {
            sb.AppendLine("""<div class="card"><div class="card-header">⚠️ Warnings</div><div style="padding:1rem 1.4rem">""");
            foreach (var w in model.Warnings)
                sb.AppendLine($"""<span class="attr-chip warn-chip">{H(w)}</span><br>""");
            sb.AppendLine("</div></div>");
        }

        sb.AppendLine("</div></body></html>");

        return sb.ToString();
    }

    private static void Row(StringBuilder sb, string label, string value, bool raw = false)
    {
        var val = raw ? value : H(value);
        sb.AppendLine($"      <tr><td>{H(label)}</td><td>{val}</td></tr>");
    }

    private static void RowIfSet(StringBuilder sb, string label, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            Row(sb, label, value);
    }

    private static void RowIfSetCopy(StringBuilder sb, string label, string value, string copyDisplay)
    {
        if (!string.IsNullOrWhiteSpace(value))
            Row(sb, label, H(value) + CopyBtn(value, copyDisplay), raw: true);
    }

    private static string CopyBtn(string value, string display)
    {
        if (display == "No") return "";
        var cls = display == "Yes on hover over" ? "copy-btn copy-hover" : "copy-btn";
        return $"""<button class="{cls}" data-copy="{H(value)}" onclick="copyFileName(this)">📋</button>""";
    }

    private static string H(string s) => HttpUtility.HtmlEncode(s);

    private static string GetFileIcon(string ext) => ext.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".tiff" or ".tif" or ".webp" or ".ico" or ".heic" or ".svg" => "🖼️",
        ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" or ".flv" => "🎬",
        ".mp3" or ".wav" or ".flac" or ".ogg" or ".aac" or ".m4a" => "🎵",
        ".pdf" => "📕",
        ".doc" or ".docx" => "📝",
        ".xls" or ".xlsx" => "📊",
        ".ppt" or ".pptx" => "📊",
        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "🗜️",
        ".exe" => "⚙️",
        ".dll" => "🔧",
        ".msi" => "📦",
        ".txt" or ".log" or ".md" => "📄",
        ".xml" or ".json" or ".yaml" or ".yml" or ".toml" => "📋",
        ".cs" or ".vb" or ".js" or ".ts" or ".py" or ".rb" or ".java" or ".cpp" or ".c" or ".go" or ".rs" => "💻",
        ".html" or ".htm" or ".css" => "🌐",
        ".sql" => "🗄️",
        ".bat" or ".cmd" or ".sh" or ".ps1" => "⚡",
        _ => "📁",
    };
}
