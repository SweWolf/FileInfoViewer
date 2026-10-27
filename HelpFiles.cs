using System.Diagnostics;
using System.Reflection;

namespace FileInfoViewer
{
    /// <summary>
    /// Opens the app's help (help.html, built from docs\help.md with docs\build-help.cmd) in the
    /// default browser. It is found in one of two ways:
    ///   1. Help\help.html next to the EXE, if the .csproj copies it there (Content item with
    ///      Link="Help\help.html").
    ///   2. Otherwise the copy embedded in the EXE (EmbeddedResource with LogicalName="Help/help.html"),
    ///      which is saved to %AppData%\SweWolfSoftware\[AppFolderName]\Help\help.html first
    ///      (rewritten when it differs, so a new version of the app brings its own help).
    ///      Use this for a single-file release that ships only the EXE.
    ///
    /// Planned: open the online help on GitHub Pages first, and keep a copy of it that is
    /// downloaded in the background, for when there is no internet.
    ///
    /// Shared with the sibling apps by copying this whole file. When copying, change the
    /// namespace and AppFolderName. When changing this file, add a line to the history below,
    /// so the copies in the other apps can be compared with it.
    ///
    /// History:
    ///   2026-09-27  Created: opens Help\help.html next to the EXE (Help > [App] Help, F1).
    ///   2026-09-27  Falls back to the help embedded in the EXE, saved to %AppData% (for
    ///               single-file releases). Added AppFolderName.
    ///   2026-10-26  Copied to FileInfoViewer (namespace and AppFolderName changed).
    /// </summary>
    internal static class HelpFiles
    {
        /// <summary>The app's folder under %AppData%\SweWolfSoftware.</summary>
        private const string AppFolderName = "FileInfoViewer";

        private const string EmbeddedHelpName = "Help/help.html";

        internal static string LocalHelpFile =>
            Path.Combine(AppContext.BaseDirectory, "Help", "help.html");

        internal static string ExtractedHelpFile =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "SweWolfSoftware", AppFolderName, "Help", "help.html");

        /// <summary>Opens the help in the default browser. <paramref name="caption"/> is the message box caption.</summary>
        internal static void OpenHelp(string caption)
        {
            string file = LocalHelpFile;
            try
            {
                if (!File.Exists(file) && ExtractEmbeddedHelp() is string extracted)
                    file = extracted;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save the help file:\n{ExtractedHelpFile}\n\n{ex.Message}",
                    caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(file))
            {
                MessageBox.Show($"The help file was not found:\n{file}",
                    caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open the help file:\n{file}\n\n{ex.Message}",
                    caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Saves the help embedded in the EXE to <see cref="ExtractedHelpFile"/>, unless the file
        /// there is already the same. Returns its path, or null if the EXE has no embedded help.
        /// </summary>
        private static string? ExtractEmbeddedHelp()
        {
            using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(EmbeddedHelpName);
            if (stream == null) return null;

            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            byte[] content = buffer.ToArray();

            string file = ExtractedHelpFile;
            if (!File.Exists(file) || !File.ReadAllBytes(file).AsSpan().SequenceEqual(content))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                File.WriteAllBytes(file, content);
            }
            return file;
        }
    }
}
