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
}
