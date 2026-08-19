using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChainOSC.Core;

namespace ChainOSC.Windows;

public static class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ChainOSC", "settings.json");

    public static ChainOscSettings Load(out string? warning)
    {
        warning = null;
        if (!File.Exists(FilePath)) return new ChainOscSettings();
        try
        {
            var json = File.ReadAllText(FilePath);
            var settings = JsonSerializer.Deserialize<ChainOscSettings>(json, JsonOptions);
            if (settings is null || settings.Keys is null)
                throw new InvalidDataException("The settings file is empty or invalid.");
            settings.Version = "0.2.0";
            return settings;
        }
        catch (Exception ex)
        {
            warning = $"Saved settings could not be loaded: {ex.Message}";
            return new ChainOscSettings();
        }
    }

    public static void Save(ChainOscSettings settings)
    {
        var directory = Path.GetDirectoryName(FilePath)!;
        Directory.CreateDirectory(directory);
        var temporaryPath = FilePath + ".tmp";
        File.WriteAllText(temporaryPath,
                          JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(temporaryPath, FilePath, true);
    }
}
