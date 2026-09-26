using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using FileInfoViewer.Models;

namespace FileInfoViewer.Services;

// Reads the root <svg> element of a vector image: size, viewBox, title, the program that made it,
// and what the file contains. DTDs are ignored and nothing external is resolved (no XXE).
internal static partial class SvgInfoReader
{
    [GeneratedRegex(@"^\s*(\d+(?:\.\d+)?)\s*(px)?\s*$")]
    private static partial Regex PixelLengthRegex();

    [GeneratedRegex(@"Generator:\s*(.+?)(?:,\s*SVG Export Plug-In.*)?\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex GeneratorRegex();

    public static void Read(string filePath, ImageInfoModel imageInfo)
    {
        imageInfo.IsVector = true;
        var details = imageInfo.FormatDetails;
        details["Format"] = "SVG (vector)";

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
            XmlResolver = null,
            IgnoreWhitespace = true,
        };
        using var reader = XmlReader.Create(filePath, settings);

        string generator = "";
        int elementCount = 0, imageCount = 0, scriptCount = 0;
        bool rootSeen = false;
        string? pending = null; // "Title" or "Description" while inside that element

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Comment when !rootSeen && generator.Length == 0:
                    // e.g. <!-- Generator: Adobe Illustrator 24.0.0, SVG Export Plug-In . SVG Version: 6.00 Build 0) -->
                    var gm = GeneratorRegex().Match(reader.Value.Trim());
                    if (gm.Success) generator = gm.Groups[1].Value.Trim();
                    break;

                case XmlNodeType.Element:
                    elementCount++;
                    if (!rootSeen)
                    {
                        if (reader.LocalName != "svg")
                            throw new InvalidDataException("Not a valid SVG file (the root element is not <svg>).");
                        rootSeen = true;
                        ReadRootAttributes(reader, imageInfo, ref generator);
                        break;
                    }
                    switch (reader.LocalName)
                    {
                        case "image": imageCount++; break;
                        case "script": scriptCount++; break;
                        case "title" when reader.Depth == 1 && !reader.IsEmptyElement: pending = "Title"; break;
                        case "desc" when reader.Depth == 1 && !reader.IsEmptyElement: pending = "Description"; break;
                    }
                    break;

                case XmlNodeType.Text or XmlNodeType.CDATA when pending != null:
                    details.TryAdd(pending, reader.Value.Trim());
                    pending = null;
                    break;

                case XmlNodeType.EndElement:
                    pending = null;
                    break;
            }
        }

        if (!rootSeen) throw new InvalidDataException("Not a valid SVG file (no <svg> element).");

        if (generator.Length > 0) details["Created With"] = generator;
        details["Element Count"] = elementCount.ToString("N0");
        details["Embedded Images"] = imageCount.ToString("N0");
        details["Contains Scripts"] = scriptCount > 0 ? $"Yes ({scriptCount})" : "No";
    }

    private static void ReadRootAttributes(XmlReader reader, ImageInfoModel imageInfo, ref string generator)
    {
        var details = imageInfo.FormatDetails;
        var version = reader.GetAttribute("version");
        var width   = reader.GetAttribute("width");
        var height  = reader.GetAttribute("height");
        var viewBox = reader.GetAttribute("viewBox");

        if (!string.IsNullOrWhiteSpace(version)) details["SVG Version"] = version.Trim();
        if (!string.IsNullOrWhiteSpace(width))   details["Width"] = width.Trim();
        if (!string.IsNullOrWhiteSpace(height))  details["Height"] = height.Trim();
        if (!string.IsNullOrWhiteSpace(viewBox)) details["View Box"] = viewBox.Trim();

        // Pixel size only when both are plain numbers or px (not %, cm, em, ...)
        if (TryParsePixels(width, out var w) && TryParsePixels(height, out var h))
        {
            imageInfo.Width = w;
            imageInfo.Height = h;
        }

        // Inkscape writes its version on the root element
        var inkscape = reader.GetAttribute("version", "http://www.inkscape.org/namespaces/inkscape");
        if (generator.Length == 0 && !string.IsNullOrWhiteSpace(inkscape))
            generator = $"Inkscape {inkscape.Trim()}";
    }

    private static bool TryParsePixels(string? value, out int pixels)
    {
        pixels = 0;
        if (value == null) return false;
        var m = PixelLengthRegex().Match(value);
        if (!m.Success || !double.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            return false;
        pixels = (int)Math.Round(d);
        return pixels > 0;
    }
}
