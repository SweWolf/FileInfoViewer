using FileInfoViewer;
using FileInfoViewer.Services;
using System.Diagnostics;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        SettingsService.ApplyStyle(SettingsService.Current.AppStyle);

        // -T<n> <file>: show template n (e.g. -T1) for the file instead of the HTML report
        if (args.Length >= 2 && TryParseTemplateSwitch(args[0], out var templateNumber))
        {
            TemplateRunner.Show(templateNumber, args[1], null);
        }
        // args[0] is the first real argument when using static Main(string[] args)
        else if (args.Length >= 1)
        {
            var filePath = args[0];
            // No main window here: run the work inside a message loop so the progress window can be shown
            var context = new ApplicationContext();
            Application.Idle += OnIdle;
            Application.Run(context);

            async void OnIdle(object? sender, EventArgs e)
            {
                Application.Idle -= OnIdle;
                try
                {
                    var model = await CollectRunner.RunAsync(filePath, null);
                    var htmlPath = HtmlReportGenerator.Generate(model);
                    BrowserLauncher.OpenInBrowser(htmlPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing file:\n\n{ex.Message}", "FileInfoViewer",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    context.ExitThread();
                }
            }
        }
        else
        {
            Application.Run(new MainForm());
        }
    }

    private static bool TryParseTemplateSwitch(string arg, out int number)
    {
        number = 0;
        return arg.Length >= 3 && arg[0] is '-' or '/' && arg[1] is 'T' or 't'
            && int.TryParse(arg.AsSpan(2), out number);
    }
}