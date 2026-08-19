namespace ChainOSC.Core;

public sealed class ChainOscSettings
{
    public string Version { get; set; } = "0.2.0";
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 9000;
    public List<KeyConfiguration> Keys { get; set; } = [KeyConfiguration.CreateDefault()];
}

public sealed class KeyConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Key 1";
    public string Hotkey { get; set; } = "F8";
    public bool Ctrl { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }
    public bool Win { get; set; }
    public string Address { get; set; } = "/avatar/parameters/ChainOSCKey";
    public OscValueType Type { get; set; } = OscValueType.Int;
    public string PressValue { get; set; } = "1";
    public string ReleaseValue { get; set; } = "0";

    public static KeyConfiguration CreateDefault() => new();

    public string HotkeyDisplay =>
        $"{(Ctrl ? "Ctrl+" : "")}{(Alt ? "Alt+" : "")}" +
        $"{(Shift ? "Shift+" : "")}{(Win ? "Win+" : "")}{Hotkey}";
}
