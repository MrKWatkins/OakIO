using System.Runtime.InteropServices;

namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class PulseSequenceBlock(Stream stream) : TzxBlock<PulseSequenceHeader>(new PulseSequenceHeader(stream), stream)
{
    public ReadOnlySpan<ushort> Pulses => MemoryMarshal.Cast<byte, ushort>(AsSpan());

    public override string ToString() => $"{Header.Type}: {string.Join(", ", Pulses.ToArray())} T-States";
}