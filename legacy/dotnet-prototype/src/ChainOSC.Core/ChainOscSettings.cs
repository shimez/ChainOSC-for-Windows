namespace ChainOSC.Core;

public enum KeyMode { PressRelease = 0, Sequence = 1 }

public sealed class ChainOscSettings
{
    public string Version { get; set; } = "0.6.0";
    public bool StartWithWindows { get; set; }
    public bool StartMinimized { get; set; } = true;
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 9000;
    public List<KeyConfiguration> Keys { get; set; } = [KeyConfiguration.CreateDefault()];
}

public sealed class OscMessageConfiguration
{
    public string Address { get; set; } = "/avatar/parameters/ChainOSCKey";
    public OscValueType Type { get; set; } = OscValueType.Int;
    public string Value { get; set; } = "1";
}

public sealed class SequenceConfiguration
{
    public string Address { get; set; } = "/avatar/parameters/ChainOSCKey";
    public OscValueType Type { get; set; } = OscValueType.Float;
    public double Start { get; set; }
    public double End { get; set; } = 10;
    public double Step { get; set; } = 1;
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
    public KeyMode Mode { get; set; }
    public List<OscMessageConfiguration> Press { get; set; } =
        [new() { Value = "1" }];
    public List<OscMessageConfiguration> Release { get; set; } =
        [new() { Value = "0" }];
    public SequenceConfiguration Sequence { get; set; } = new();

    public static KeyConfiguration CreateDefault() => new();

    public string HotkeyDisplay => string.IsNullOrWhiteSpace(Hotkey)
        ? "Not assigned"
        : $"{(Ctrl ? "Ctrl+" : "")}{(Alt ? "Alt+" : "")}" +
          $"{(Shift ? "Shift+" : "")}{(Win ? "Win+" : "")}{Hotkey}";
}
