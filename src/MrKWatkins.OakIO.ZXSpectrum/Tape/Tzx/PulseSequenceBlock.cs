using System.Runtime.InteropServices;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block containing a sequence of pulses with arbitrary durations.
/// </summary>
public sealed class PulseSequenceBlock : TzxBlock<PulseSequenceHeader>
{
    internal PulseSequenceBlock(byte[] headerData, byte[] data) : base(new PulseSequenceHeader(headerData), data)
    {
    }

    /// <summary>
    /// Gets the pulse durations in T-states.
    /// </summary>
    public ReadOnlySpan<ushort> Pulses => MemoryMarshal.Cast<byte, ushort>(AsSpan());

    /// <inheritdoc />
    [Pure]
    public override string ToString() => $"{Header.Type}: {string.Join(", ", Pulses.ToArray())} T-States";
}