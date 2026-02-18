using System.Runtime.InteropServices;

namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class PulseSequenceBlock : TzxBlock<PulseSequenceHeader>
{
    public PulseSequenceBlock(Stream stream) : base(new PulseSequenceHeader(stream), stream)
    {
    }

    internal PulseSequenceBlock(byte[] headerData, byte[] data) : base(new PulseSequenceHeader(headerData), data)
    {
    }

    public ReadOnlySpan<ushort> Pulses => MemoryMarshal.Cast<byte, ushort>(AsSpan());

    public override string ToString() => $"{Header.Type}: {string.Join(", ", Pulses.ToArray())} T-States";
}