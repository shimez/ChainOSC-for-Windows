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
            if (settings.Version == "0.2.0") MigrateV02(json, settings);
            settings.Version = "0.3.0";
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

    private static void MigrateV02(string json, ChainOscSettings settings)
    {
        using var document = JsonDocument.Parse(json);
        var sourceKeys = document.RootElement.GetProperty("keys");
        for (var index = 0; index < settings.Keys.Count && index < sourceKeys.GetArrayLength();
             ++index)
        {
            var source = sourceKeys[index];
            var key = settings.Keys[index];
            var address = source.TryGetProperty("address", out var addressValue)
                ? addressValue.GetString() ?? "/avatar/parameters/ChainOSCKey"
                : "/avatar/parameters/ChainOSCKey";
            var typeText = source.TryGetProperty("type", out var typeValue)
                ? typeValue.GetString() ?? "int" : "int";
            if (!Enum.TryParse<OscValueType>(typeText, true, out var type))
                type = OscValueType.Int;
            var pressValue = source.TryGetProperty("pressValue", out var press)
                ? press.GetString() ?? "1" : "1";
            var releaseValue = source.TryGetProperty("releaseValue", out var release)
                ? release.GetString() ?? "0" : "0";
            key.Press = [new OscMessageConfiguration
                { Address = address, Type = type, Value = pressValue }];
            key.Release = [new OscMessageConfiguration
                { Address = address, Type = type, Value = releaseValue }];
            key.Sequence = new SequenceConfiguration { Address = address };
        }
    }
}
