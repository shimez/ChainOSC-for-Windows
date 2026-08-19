"""Minimal OSC receiver for ChainOSC for Windows v0.1.0 testing."""

import socket
import struct


def osc_string(packet: bytes, offset: int) -> tuple[str, int]:
    end = packet.index(0, offset)
    text = packet[offset:end].decode("utf-8")
    return text, (end + 4) & ~3


with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as receiver:
    receiver.bind(("0.0.0.0", 9000))
    print("Listening for OSC on UDP 0.0.0.0:9000 (Ctrl+C to stop)")
    while True:
        packet, remote = receiver.recvfrom(65535)
        try:
            address, offset = osc_string(packet, 0)
            tag, offset = osc_string(packet, offset)
            if tag == ",i":
                value = struct.unpack_from(">i", packet, offset)[0]
            elif tag == ",f":
                value = struct.unpack_from(">f", packet, offset)[0]
            elif tag == ",s":
                value, _ = osc_string(packet, offset)
            else:
                value = f"unsupported tag {tag}"
            print(f"{remote[0]}:{remote[1]}  {address}  {tag}  {value}")
        except (ValueError, UnicodeDecodeError, struct.error) as error:
            print(f"{remote[0]}:{remote[1]}  invalid OSC packet: {error}")
