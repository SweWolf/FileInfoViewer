using System.Text.Json;
using FileInfoViewer.Models;

namespace FileInfoViewer.Services;

public static class SettingsService
{
    private static readonly string SettingsFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SweWolfSoftware", "FileInfoViewer");

    private static readonly string SettingsFilePath =
        Path.Combine(SettingsFolder, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static AppSettings? _current;

    /// <summary>Returns the current settings, loading from disk on first access.</summary>
    public static AppSettings Current => _current ??= Load();

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                _current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                return _current;
            }
        }
        catch { }

        _current = new AppSettings();
        return _current;
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsFolder);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsFilePath, json);
            _current = settings;
        }
        catch { }
    }

#pragma warning disable WFO5001
    public static void ApplyStyle(string style)
    {
        Application.SetColorMode(style switch
        {
            "Dark"  => SystemColorMode.Dark,
            "Light" => SystemColorMode.Classic,
            _       => SystemColorMode.System,
        });
        foreach (Form form in Application.OpenForms)
        {
            FixButtonStyles(form);
            form.Invalidate(true);
            form.Refresh();
        }
    }
#pragma warning restore WFO5001

    // Switches all buttons to FlatStyle.System so they render correctly in dark mode.
    // FlatStyle.System uses native OS rendering which handles dark mode; Standard does not.
    public static void FixButtonStyles(Control root)
    {
        if (root is Button btn) btn.FlatStyle = FlatStyle.System;
        foreach (Control child in root.Controls)
            FixButtonStyles(child);
    }
}
