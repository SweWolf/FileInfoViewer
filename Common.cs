namespace FileInfoViewer;

/// <summary>
/// General-purpose utility methods that are candidates for reuse across projects.
/// Copy this file into other projects and adjust the namespace as needed.
/// </summary>
internal static class Common
{
    /// <summary>
    /// Creates a Windows shortcut (.lnk) file at the specified path.
    /// </summary>
    /// <param name="shortcutPath">Full path of the .lnk file to create.</param>
    /// <param name="targetPath">Path to the executable the shortcut points to.</param>
    /// <param name="description">Tooltip description shown in Explorer.</param>
    /// <param name="arguments">Optional command-line arguments (e.g. "%1" for Send To).</param>
    public static void CreateShortcut(string shortcutPath, string targetPath,
        string description = "", string arguments = "")
    {
        Type shellType = Type.GetTypeFromProgID("WScript.Shell")!;
        dynamic shell  = Activator.CreateInstance(shellType)!;
        dynamic lnk    = shell.CreateShortcut(shortcutPath);
        lnk.TargetPath       = targetPath;
        lnk.Arguments        = arguments;
        lnk.WorkingDirectory = Path.GetDirectoryName(targetPath) ?? "";
        lnk.Description      = description;
        lnk.IconLocation     = targetPath;
        lnk.Save();
    }

    /// <summary>
    /// Opens a URL or file path in the default application (browser, viewer, etc.).
    /// </summary>
    public static void OpenWithDefaultApp(string url)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true,
        });
    }

    /// <summary>
    /// Checks GitHub for a newer release and prompts the user to go to the download page.
    /// Fails silently if the network is unavailable or the check fails for any reason.
    /// </summary>
    /// <param name="owner">GitHub username or organisation (e.g. "SweWolf")</param>
    /// <param name="repo">GitHub repository name (e.g. "FileInfoViewer")</param>
    public static async Task CheckForUpdatesAsync(string owner, string repo)
    {
        var currentVersion = System.Reflection.Assembly
            .GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);

        var result = await GitHubUpdateChecker.CheckAsync(owner, repo, currentVersion);

        if (result is { IsUpdateAvailable: true })
        {
            var answer = MessageBox.Show(
                $"A new version is available: {result.LatestVersion}\n\n" +
                $"You are running version {currentVersion.Major}.{currentVersion.Minor}.{currentVersion.Build}.\n\n" +
                $"Do you want to go to the download page?",
                "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (answer == DialogResult.Yes)
                OpenWithDefaultApp(result.ReleasePageUrl);
        }
    }

    /// <summary>
    /// Formats a byte count as a human-readable size string (B / KB / MB / GB).
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}
