using FileInfoViewer;
using FileInfoViewer.Services;
using System.Diagnostics;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        // args[0] is the first real argument when using static Main(string[] args)
        if (args.Length >= 1)
        {
            var filePath = args[0];
            try
            {
                var model = FileInfoCollector.Collect(filePath);
                var htmlPath = HtmlReportGenerator.Generate(model);
                BrowserLauncher.OpenInBrowser(htmlPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing file:\n\n{ex.Message}", "FileInfoViewer",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            Application.Run(new MainForm());
        }
    }
}