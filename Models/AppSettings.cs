namespace FileInfoViewer.Models;

public class AppSettings
{
    /// <summary>Show seconds in all timestamp fields in the HTML report.</summary>
    public bool ShowSeconds { get; set; } = false;

    /// <summary>Which timezone to display for timestamps: "Local", "UTC", or "Both".</summary>
    public string TimeZoneDisplay { get; set; } = "Local";

    /// <summary>Whether to show a copy button next to the file name: "No", "Yes", or "Yes on hover over".</summary>
    public string CopyButtonDisplay { get; set; } = "Yes on hover over";

    /// <summary>Whether to show the Owner row in the HTML report.</summary>
    public bool ShowOwner { get; set; } = false;

    /// <summary>Whether to show the File Attributes section in the HTML report.</summary>
    public bool ShowFileAttributes { get; set; } = false;

    /// <summary>Whether to show the File Hashes section in the HTML report.</summary>
    public bool ShowFileHashes { get; set; } = true;

    /// <summary>How to display textual/JSON metadata: "None", "Formatted", "Raw data", or "Both Formatted and Raw Data".</summary>
    public string TextualDataDisplay { get; set; } = "Formatted";

    /// <summary>Whether to render values starting with http:// or https:// as clickable hyperlinks.</summary>
    public bool WebLinksClickable { get; set; } = true;

    /// <summary>Max width of the HTML report content area: "Narrow (800px)", "Normal (1100px)", "Wide (1400px)", "Very wide (1800px)", "Full width", "Custom".</summary>
    public string ContentMaxWidth { get; set; } = "Normal (1100px)";

    /// <summary>Numeric part of the custom content width (e.g. "1200").</summary>
    public string CustomContentWidth { get; set; } = "1200";

    /// <summary>Unit for the custom content width: "px" or "%".</summary>
    public string CustomContentWidthUnit { get; set; } = "px";
}
