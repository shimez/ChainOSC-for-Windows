using System.Text.Json;

namespace ChainOSC.Core;

public static class KeyPresetCodec
{
    public const string Format = "ChainOSC-device-preset";
    public const string LegacyFormat = "M5ChainOSC-device-preset";
    public const int SchemaVersion = 1;
    public const int KeyDeviceType = 3;

    public static string Export(KeyConfiguration configuration)
    {
        var preset = new
        {
            format = Format,
            schemaVersion = SchemaVersion,
            deviceType = KeyDeviceType,
            deviceTypeName = "Key",
            key = new
            {
                mode = (int)configuration.Mode,
                press = configuration.Press.Select(MessageObject),
                release = configuration.Release.Select(MessageObject),
                sequence = new
                {
                    address = configuration.Sequence.Address,
                    type = (int)configuration.Sequence.Type,
                    start = configuration.Sequence.Start,
                    end = configuration.Sequence.End,
                    step = configuration.Sequence.Step,
                },
            },
        };
        return JsonSerializer.Serialize(preset, new JsonSerializerOptions
        {
            WriteIndented = true,
        });
    }

    public static void Apply(string json, KeyConfiguration destination)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var format = RequiredString(root, "format");
        if (format != Format && format != LegacyFormat)
            throw new InvalidDataException("This is not a supported ChainOSC device preset.");
        if (RequiredInt(root, "schemaVersion") != SchemaVersion)
            throw new InvalidDataException("Unsupported preset schemaVersion.");
        if (RequiredInt(root, "deviceType") != KeyDeviceType)
            throw new InvalidDataException("Device type mismatch. Select a Key preset.");
        if (!root.TryGetProperty("key", out var key) ||
            key.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Key settings are missing.");

        var mode = RequiredInt(key, "mode");
        if (mode is < 0 or > 1)
            throw new InvalidDataException("Key mode is invalid.");
        var press = ReadMessages(key, "press");
        var release = ReadMessages(key, "release");
        if (press.Count + release.Count > 8)
            throw new InvalidDataException("Key messages exceed the limit of 8.");
        if (!key.TryGetProperty("sequence", out var sequence) ||
            sequence.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Sequence settings are missing.");

        var sequenceType = ReadType(RequiredInt(sequence, "type"));
        var importedSequence = new SequenceConfiguration
        {
            Address = RequiredString(sequence, "address"),
            Type = sequenceType,
            Start = RequiredDouble(sequence, "start"),
            End = RequiredDouble(sequence, "end"),
            Step = RequiredDouble(sequence, "step"),
        };

        destination.Mode = (KeyMode)mode;
        destination.Press = press;
        destination.Release = release;
        destination.Sequence = importedSequence;
    }

    private static object MessageObject(OscMessageConfiguration message) => new
    {
        address = message.Address,
        value = message.Value,
        type = (int)message.Type,
    };

    private static List<OscMessageConfiguration> ReadMessages(JsonElement key,
                                                               string name)
    {
        if (!key.TryGetProperty(name, out var array) ||
            array.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException($"Key {name} messages are missing.");
        var result = new List<OscMessageConfiguration>();
        foreach (var item in array.EnumerateArray())
        {
            result.Add(new OscMessageConfiguration
            {
                Address = RequiredString(item, "address"),
                Value = RequiredString(item, "value"),
                Type = ReadType(RequiredInt(item, "type")),
            });
        }
        return result;
    }

    private static OscValueType ReadType(int value) => value switch
    {
        0 => OscValueType.Float,
        1 => OscValueType.Int,
        2 => OscValueType.String,
        _ => throw new InvalidDataException("OSC value type is invalid."),
    };

    private static string RequiredString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString()!
            : throw new InvalidDataException($"{name} is missing or invalid.");

    private static int RequiredInt(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.TryGetInt32(out var result)
            ? result
            : throw new InvalidDataException($"{name} is missing or invalid.");

    private static double RequiredDouble(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.TryGetDouble(out var result) &&
        double.IsFinite(result)
            ? result
            : throw new InvalidDataException($"{name} is missing or invalid.");
}
