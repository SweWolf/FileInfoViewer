using System.Diagnostics;

namespace FileInfoViewer.Services;

public static class BrowserLauncher
{
    public static void OpenInBrowser(string htmlFilePath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = htmlFilePath,
            UseShellExecute = true,
        });
    }
}
