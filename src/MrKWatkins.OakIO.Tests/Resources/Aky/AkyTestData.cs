using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.Tests.Resources.Aky;

internal static class AkyTestData
{
    [Pure]
    internal static byte[] Create(byte[]? packets = null, int duration = 1, int channels = 3, bool littleEndian = true, ushort origin = 0)
    {
        packets ??= [0];
        var header = 2 + ((channels + 2) / 3 * 4);
        var track = header + 2 + (channels * 2) + 4;
        var block = track + (channels * 3);
        var bytes = new byte[block + packets.Length + duration];
        var endian = littleEndian ? Endian.Little : Endian.Big;
        bytes[0] = (byte)(1 | (littleEndian ? 128 : 0));
        bytes[1] = (byte)channels;
        for (var chip = 0; chip < (channels + 2) / 3; ++chip)
        {
            bytes.SetUInt32(2 + (chip * 4), (uint)(1773400 + chip), endian);
        }
        bytes.SetUInt16(header, (ushort)duration, endian);
        for (var channel = 0; channel < channels; ++channel)
        {
            bytes.SetUInt16(header + 2 + (channel * 2), (ushort)(origin + track + (channel * 3)), endian);
            bytes[track + (channel * 3)] = (byte)duration;
            bytes.SetUInt16(track + (channel * 3) + 1, (ushort)(origin + block + (channel == 0 ? 0 : packets.Length)), endian);
        }
        bytes.SetUInt16(header + 2 + (channels * 2) + 2, (ushort)(origin + header), endian);
        packets.CopyTo(bytes, block);
        return bytes;
    }

    [Pure]
    internal static int BlockOffset(int channels = 3) => 2 + ((channels + 2) / 3 * 4) + 2 + (channels * 2) + 4 + (channels * 3);
}