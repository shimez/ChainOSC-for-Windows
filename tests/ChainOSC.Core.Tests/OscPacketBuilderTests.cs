using ChainOSC.Core;

namespace ChainOSC.Core.Tests;

public class OscPacketBuilderTests
{
    [Fact]
    public void BuildsIntPacketWithBigEndianValue()
    {
        var packet = OscPacketBuilder.Build(new OscMessage("/test", OscValueType.Int, "1"));
        Assert.Equal(new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0, 0, 0,
                                  0x2C, 0x69, 0, 0, 0, 0, 0, 1 }, packet);
    }

    [Fact]
    public void RejectsAddressWithoutLeadingSlash() =>
        Assert.Throws<ArgumentException>(() => OscPacketBuilder.Build(
            new OscMessage("test", OscValueType.Int, "1")));

    [Fact]
    public void KeyPresetUsesSharedChainOscFormatAndRoundTrips()
    {
        var source = KeyConfiguration.CreateDefault();
        source.Mode = KeyMode.PressRelease;
        source.Press.Add(new OscMessageConfiguration
        {
            Address = "/second",
            Type = OscValueType.String,
            Value = "hello",
        });
        var json = KeyPresetCodec.Export(source);
        var destination = KeyConfiguration.CreateDefault();
        destination.Name = "Keep this name";
        destination.Hotkey = "F9";

        KeyPresetCodec.Apply(json, destination);

        Assert.Contains("\"format\": \"ChainOSC-device-preset\"", json);
        Assert.Equal(2, destination.Press.Count);
        Assert.Equal("/second", destination.Press[1].Address);
        Assert.Equal("Keep this name", destination.Name);
        Assert.Equal("F9", destination.Hotkey);
    }

    [Fact]
    public void KeyPresetRejectsAnotherDeviceType()
    {
        const string json = """
            {"format":"ChainOSC-device-preset","schemaVersion":1,
             "deviceType":1,"deviceTypeName":"Encoder","key":{}}
            """;
        Assert.Throws<InvalidDataException>(() =>
            KeyPresetCodec.Apply(json, KeyConfiguration.CreateDefault()));
    }

    [Fact]
    public void ImportsPresetExportedByM5ChainOsc()
    {
        const string json = """
            {"format":"ChainOSC-device-preset","schemaVersion":1,
             "deviceType":3,"deviceTypeName":"Key","key":{"mode":0,
             "press":[{"address":"/input/AFKToggle","value":"1","type":1}],
             "release":[{"address":"/input/AFKToggle","value":"0","type":1}],
             "sequence":{"address":"/avatar/parameters/KeySeq","type":0,
             "start":0.000000,"end":10.000000,"step":1.000000}}}
            """;
        var destination = KeyConfiguration.CreateDefault();

        KeyPresetCodec.Apply(json, destination);

        Assert.Equal(KeyMode.PressRelease, destination.Mode);
        Assert.Equal("/input/AFKToggle", destination.Press.Single().Address);
        Assert.Equal(OscValueType.Int, destination.Press.Single().Type);
        Assert.Equal("1", destination.Press.Single().Value);
        Assert.Equal("0", destination.Release.Single().Value);
    }
}
