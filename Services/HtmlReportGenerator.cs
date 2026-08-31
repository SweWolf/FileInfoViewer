using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        var tzDisplay    = settings.TimeZoneDisplay;
        var contentWidth = settings.ContentMaxWidth switch {
            "Narrow (800px)"    => "800px",
            "Wide (1400px)"     => "1400px",
            "Very wide (1800px)"=> "1800px",
            "Full width"        => "100%",
            "Custom"            => ResolveCustomWidth(settings.CustomContentWidth, settings.CustomContentWidthUnit),
            _                   => "1100px",
        };

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
  .content { max-width: {{contentWidth}}; margin: 2rem auto; padding: 0 1.5rem 3rem; }
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
  .json-block { background: #f8f9fc; border: 1px solid #e0e4ef; border-radius: 6px;
                padding: .75rem 1rem; font-family: 'Consolas','Courier New',monospace;
                font-size: .82rem; line-height: 1.55; white-space: pre-wrap; word-break: break-word;
                max-height: 420px; overflow-y: auto; color: #333; display: block; margin: .1rem 0; }
  .jk { color: #7b2fbe; } .js { color: #1a5276; } .jn { color: #c0392b; }
  .jb { color: #1565c0; } .jz { color: #999; }
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
var webLinksClickable={{(settings.WebLinksClickable?"true":"false")}};
function esc(s){return s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');}
function trimUrl(u){for(;;){if(!u.length)break;var l=u[u.length-1];if(l==='.'||l===','||l===';'||l===':'){u=u.slice(0,-1);continue;}if(l===')'){var a=(u.match(/\(/g)||[]).length,b=(u.match(/\)/g)||[]).length;if(b>a){u=u.slice(0,-1);continue;} }if(l===']'){var c=(u.match(/\[/g)||[]).length,d=(u.match(/\]/g)||[]).length;if(d>c){u=u.slice(0,-1);continue;} }break;}return u;}
function linkifyInStr(raw){var re=/(https?:\/\/[^\s"\\]+)/g,out='',last=0,m;while((m=re.exec(raw))!==null){out+=esc(raw.slice(last,m.index));var u=trimUrl(m[1]);out+='<a href="'+esc(u)+'" target="_blank" rel="noopener noreferrer">'+esc(u)+'</a>'+esc(m[1].slice(u.length));last=m.index+m[0].length;}return out+esc(raw.slice(last));}
function highlightJson(el){
  var t=el.textContent;
  el.innerHTML=t.replace(/("(?:\\u[0-9a-fA-F]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(?:true|false)\b|\bnull\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g,function(m){
    if(/^"/.test(m)){if(/:$/.test(m))return'<span class="jk">'+esc(m)+'</span>';var inner=m.slice(1,m.length-1);return'<span class="js">"'+(webLinksClickable?linkifyInStr(inner):esc(inner))+'"</span>';}
    if(m==='true'||m==='false')return'<span class="jb">'+m+'</span>';
    if(m==='null')return'<span class="jz">'+m+'</span>';
    return'<span class="jn">'+m+'</span>';
  });
}
document.addEventListener('DOMContentLoaded',function(){document.querySelectorAll('.json-block').forEach(highlightJson);});
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

            var textualMode = SettingsService.Current.TextualDataDisplay;
            var showRaw       = textualMode is "Raw data" or "Both Formatted and Raw Data";
            var showFormatted = textualMode is "Formatted" or "Both Formatted and Raw Data";

            var exifEntries = img.ExifTags
                .Where(kv => showRaw || !kv.Key.StartsWith("PNG-tEXt", StringComparison.OrdinalIgnoreCase))
                .OrderBy(kv => kv.Key)
                .ToList();

            if (exifEntries.Count > 0)
            {
                sb.AppendLine($"""  <div class="card-header" style="border-top:1px solid #e8eaf0">📷 EXIF / Metadata</div><table class="tag-table">""");
                foreach (var (key, value) in exifEntries)
                {
                    var jsonHtml = TryRenderJson(value, copyDisplay);
                    if (jsonHtml != null)
                        Row(sb, key, jsonHtml, raw: true);
                    else
                        Row(sb, key, SmartFormatTagValue(key, value) + CopyBtn(value, copyDisplay), raw: true);
                }
                sb.AppendLine("  </table>");
            }

            if (showFormatted && img.PngTextChunks.Count > 0)
            {
                sb.AppendLine($"""  <div class="card-header" style="border-top:1px solid #e8eaf0">📝 PNG Text Chunks</div><table class="tag-table">""");
                foreach (var (keyword, text) in img.PngTextChunks.OrderBy(x => x.Key))
                {
                    var jsonHtml = TryRenderJson(text, copyDisplay);
                    if (jsonHtml != null)
                        Row(sb, keyword, jsonHtml, raw: true);
                    else
                        Row(sb, keyword, MaybeLinkify(text) + CopyBtn(text, copyDisplay), raw: true);
                }
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
                    Row(sb, key, SmartFormatTagValue(key, value) + CopyBtn(value, copyDisplay), raw: true);
                sb.AppendLine("  </table>");
            }

            sb.AppendLine("</div>");
        }

        // Video info card
        if (model.VideoInfo is { } vid)
        {
            sb.AppendLine("""
<div class="card">
  <div class="card-header">🎬 Video Information</div>
""");
            // Description group (matches Windows Explorer Details tab)
            bool hasDesc = !string.IsNullOrEmpty(vid.Title) || !string.IsNullOrEmpty(vid.Subject)
                        || !string.IsNullOrEmpty(vid.Comment) || !string.IsNullOrEmpty(vid.Tags)
                        || !string.IsNullOrEmpty(vid.Rating);
            if (hasDesc)
            {
                sb.AppendLine("""  <div class="card-header" style="font-size:.82rem;color:#888;padding:.4rem 1.4rem;background:#fafbff;border-bottom:1px solid #f0f2f5">Description</div><table>""");
                RowIfSetCopy(sb, "Title",    vid.Title,   copyDisplay);
                RowIfSet(sb, "Subtitle", vid.Subject);
                RowJsonOrCopy(sb, "Comment",  vid.Comment, copyDisplay);
                RowIfSetCopy(sb, "Tags",     vid.Tags,    copyDisplay);
                RowIfSet(sb, "Rating",   vid.Rating);
                sb.AppendLine("  </table>");
            }

            // Video group
            bool hasVideo = !string.IsNullOrEmpty(vid.Duration) || vid.Width > 0 || !string.IsNullOrEmpty(vid.FrameRate)
                         || !string.IsNullOrEmpty(vid.DataRate) || !string.IsNullOrEmpty(vid.TotalBitrate)
                         || !string.IsNullOrEmpty(vid.VideoCodec);
            if (hasVideo)
            {
                sb.AppendLine("""  <div class="card-header" style="font-size:.82rem;color:#888;padding:.4rem 1.4rem;background:#fafbff;border-top:1px solid #e8eaf0;border-bottom:1px solid #f0f2f5">Video</div><table>""");
                RowIfSet(sb, "Length",       vid.Duration);
                if (vid.Width > 0 && vid.Height > 0)
                    Row(sb, "Frame size", $"{vid.Width} × {vid.Height}");
                RowIfSet(sb, "Frame rate",    vid.FrameRate);
                RowIfSet(sb, "Data rate",     vid.DataRate);
                RowIfSet(sb, "Total bitrate", vid.TotalBitrate);
                RowIfSet(sb, "Codec",         vid.VideoCodec);
                sb.AppendLine("  </table>");
            }

            // Audio group
            bool hasAudio = !string.IsNullOrEmpty(vid.AudioBitrate) || !string.IsNullOrEmpty(vid.AudioChannels)
                         || !string.IsNullOrEmpty(vid.AudioSampleRate);
            if (hasAudio)
            {
                sb.AppendLine("""  <div class="card-header" style="font-size:.82rem;color:#888;padding:.4rem 1.4rem;background:#fafbff;border-top:1px solid #e8eaf0;border-bottom:1px solid #f0f2f5">Audio</div><table>""");
                RowIfSet(sb, "Bit rate",         vid.AudioBitrate);
                RowIfSet(sb, "Channels",         vid.AudioChannels);
                RowIfSet(sb, "Audio sample rate", vid.AudioSampleRate);
                sb.AppendLine("  </table>");
            }

            // Extra tags from TagLib# (Creator, Year, Genre, Copyright)
            bool hasExtra = !string.IsNullOrEmpty(vid.Creator) || !string.IsNullOrEmpty(vid.Year)
                         || !string.IsNullOrEmpty(vid.Genre)   || !string.IsNullOrEmpty(vid.Copyright);
            if (hasExtra)
            {
                sb.AppendLine("""  <div class="card-header" style="font-size:.82rem;color:#888;padding:.4rem 1.4rem;background:#fafbff;border-top:1px solid #e8eaf0;border-bottom:1px solid #f0f2f5">Extra</div><table>""");
                RowIfSet(sb, "Creator",   vid.Creator);
                RowIfSet(sb, "Year",      vid.Year);
                RowIfSet(sb, "Genre",     vid.Genre);
                RowIfSet(sb, "Copyright", vid.Copyright);
                sb.AppendLine("  </table>");
            }

            if (!string.IsNullOrWhiteSpace(vid.Lyrics))
            {
                sb.AppendLine("""  <div class="hover-parent">""");
                sb.AppendLine($"""    <div class="card-header" style="border-top:1px solid #e8eaf0">🎤 Lyrics{CopyBtn(vid.Lyrics, copyDisplay)}</div>""");
                sb.AppendLine($"""    <div style="padding:1rem 1.4rem;white-space:pre-wrap;font-size:.9rem;line-height:1.8;color:#333">{H(vid.Lyrics)}</div>""");
                sb.AppendLine("""  </div>""");
            }

            if (vid.AllTags.Count > 0)
            {
                sb.AppendLine($"""  <div class="card-header" style="border-top:1px solid #e8eaf0">🏷️ All Metadata Tags</div><table class="tag-table">""");
                foreach (var (key, value) in vid.AllTags.OrderBy(x => x.Key))
                {
                    var jsonHtml = TryRenderJson(value, copyDisplay);
                    if (jsonHtml != null)
                        Row(sb, key, jsonHtml, raw: true);
                    else
                        Row(sb, key, SmartFormatTagValue(key, value) + CopyBtn(value, copyDisplay), raw: true);
                }
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
            Row(sb, label, MaybeLinkify(value), raw: true);
    }

    private static void RowIfSetCopy(StringBuilder sb, string label, string value, string copyDisplay)
    {
        if (!string.IsNullOrWhiteSpace(value))
            Row(sb, label, MaybeLinkify(value) + CopyBtn(value, copyDisplay), raw: true);
    }

    private static void RowJsonOrCopy(StringBuilder sb, string label, string value, string copyDisplay)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        var jsonHtml = TryRenderJson(value, copyDisplay);
        if (jsonHtml != null)
            Row(sb, label, jsonHtml, raw: true);
        else
            Row(sb, label, H(value) + CopyBtn(value, copyDisplay), raw: true);
    }

    private static readonly JsonDocumentOptions _lenientJson = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    private static readonly JsonSerializerOptions _prettyJson = new()
    {
        WriteIndented = true,
    };

    private static string? TryRenderJson(string value, string copyDisplay)
    {
        var trimmed = value.Trim();
        if (!trimmed.StartsWith('{') && !trimmed.StartsWith('[')) return null;
        try
        {
            using var doc = JsonDocument.Parse(trimmed, _lenientJson);
            var pretty = JsonSerializer.Serialize(doc.RootElement, _prettyJson);
            return $"""<code class="json-block">{H(pretty)}</code>{CopyBtn(pretty, copyDisplay)}""";
        }
        catch
        {
            return $"""<code class="json-block">{H(trimmed)}</code>{CopyBtn(trimmed, copyDisplay)}""";
        }
    }

    private static string CopyBtn(string value, string display)
    {
        if (display == "No") return "";
        var cls = display == "Yes on hover over" ? "copy-btn copy-hover" : "copy-btn";
        return $"""<button class="{cls}" data-copy="{H(value)}" onclick="copyFileName(this)">📋</button>""";
    }

    private static string H(string s) => HttpUtility.HtmlEncode(s);

    private static readonly Regex _urlRx =
        new(@"https?://[^\s""'<>\\]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Strip trailing punctuation that is likely not part of the URL.
    private static string ResolveCustomWidth(string value, string unit)
    {
        if (unit == "%")
        {
            if (int.TryParse(value, out int pct) && pct >= 1 && pct <= 100)
                return $"{pct}%";
        }
        else
        {
            if (int.TryParse(value, out int px) && px >= 100 && px <= 9999)
                return $"{px}px";
        }
        return "1100px"; // fallback for invalid input
    }

    // Handles unbalanced closing parens/brackets (e.g. markdown "[text](url)").
    private static string TrimUrlTrail(string url)
    {
        bool changed;
        do {
            changed = false;
            if (url.Length == 0) break;
            char last = url[^1];
            if (last is '.' or ',' or ';' or ':') { url = url[..^1]; changed = true; }
            else if (last == ')' && url.Count(c => c == ')') > url.Count(c => c == '('))
                { url = url[..^1]; changed = true; }
            else if (last == ']' && url.Count(c => c == ']') > url.Count(c => c == '['))
                { url = url[..^1]; changed = true; }
        } while (changed);
        return url;
    }

    private static string MaybeLinkify(string value)
    {
        if (!SettingsService.Current.WebLinksClickable)
            return H(value);

        var matches = _urlRx.Matches(value);
        if (matches.Count == 0)
            return H(value);

        var sb = new StringBuilder();
        int last = 0;
        foreach (Match m in matches)
        {
            sb.Append(H(value[last..m.Index]));
            var url = TrimUrlTrail(m.Value);
            sb.Append($"""<a href="{H(url)}" target="_blank" rel="noopener noreferrer">{H(url)}</a>""");
            if (url.Length < m.Length) sb.Append(H(m.Value[url.Length..]));
            last = m.Index + m.Length;
        }
        sb.Append(H(value[last..]));
        return sb.ToString();
    }

    // Format a tag value with thousand separators when it looks like a large integer,
    // but leave it untouched when the key suggests it's a seed, hash, or identifier.
    private static string SmartFormatTagValue(string key, string value)
    {
        var lk = key.ToLowerInvariant();
        if (lk.Contains("seed") || lk.Contains("hash") || lk.Contains("crc") ||
            lk.Contains("checksum") || lk.Contains("unique id") || lk.Contains("uniqueid"))
            return H(value);

        var trimmed = value.Trim();
        var spaceIdx = trimmed.IndexOf(' ');
        var numPart  = spaceIdx > 0 ? trimmed[..spaceIdx] : trimmed;
        var unitPart = spaceIdx > 0 ? trimmed[spaceIdx..] : "";  // keeps the leading space

        if (long.TryParse(numPart, out long num) && num >= 10_000)
            return H($"{num:N0}{unitPart}");

        return MaybeLinkify(value);
    }

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
