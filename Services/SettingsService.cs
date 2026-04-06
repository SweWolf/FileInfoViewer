using System.Text.Json;
using FileInfoViewer.Models;

namespace FileInfoViewer.Services;

public static class SettingsService
{
    private static readonly string SettingsFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileInfoViewer");

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
}
