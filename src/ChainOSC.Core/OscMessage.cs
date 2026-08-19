namespace ChainOSC.Core;

public enum OscValueType { Float, Int, String }

public sealed record OscMessage(string Address, OscValueType Type, string Value);
