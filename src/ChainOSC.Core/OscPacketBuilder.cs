using System.Buffers.Binary;
using System.Globalization;
using System.Text;

namespace ChainOSC.Core;

public static class OscPacketBuilder
{
    public static byte[] Build(OscMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Address) ||
            !message.Address.StartsWith("/", StringComparison.Ordinal))
            throw new ArgumentException("OSC Address must start with '/'.", nameof(message));

        using var stream = new MemoryStream();
        WriteOscString(stream, message.Address);
        switch (message.Type)
        {
            case OscValueType.Int:
                WriteOscString(stream, ",i");
                if (!int.TryParse(message.Value, NumberStyles.Integer,
                                  CultureInfo.InvariantCulture, out var intValue))
                    throw new ArgumentException("Value is not a valid Int.", nameof(message));
                Span<byte> intBytes = stackalloc byte[4];
                BinaryPrimitives.WriteInt32BigEndian(intBytes, intValue);
                stream.Write(intBytes);
                break;
            case OscValueType.Float:
                WriteOscString(stream, ",f");
                if (!float.TryParse(message.Value, NumberStyles.Float,
                                    CultureInfo.InvariantCulture, out var floatValue) ||
                    !float.IsFinite(floatValue))
                    throw new ArgumentException("Value is not a valid Float.", nameof(message));
                Span<byte> floatBytes = stackalloc byte[4];
                BinaryPrimitives.WriteInt32BigEndian(
                    floatBytes, BitConverter.SingleToInt32Bits(floatValue));
                stream.Write(floatBytes);
                break;
            case OscValueType.String:
                WriteOscString(stream, ",s");
                WriteOscString(stream, message.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(message));
        }
        return stream.ToArray();
    }

    private static void WriteOscString(Stream stream, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        stream.Write(bytes);
        stream.WriteByte(0);
        while (stream.Position % 4 != 0) stream.WriteByte(0);
    }
}
