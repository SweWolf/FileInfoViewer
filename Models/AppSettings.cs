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
}
