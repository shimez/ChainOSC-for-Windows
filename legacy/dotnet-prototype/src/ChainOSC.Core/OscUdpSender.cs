using System.Net.Sockets;

namespace ChainOSC.Core;

public sealed class OscUdpSender : IDisposable
{
    private readonly UdpClient _client = new();

    public async Task SendAsync(string host, int port, OscMessage message,
                                CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("OSC host is required.", nameof(host));
        if (port is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(port));
        var packet = OscPacketBuilder.Build(message);
        await _client.SendAsync(packet, host, port, cancellationToken);
    }

    public void Dispose() => _client.Dispose();
}
