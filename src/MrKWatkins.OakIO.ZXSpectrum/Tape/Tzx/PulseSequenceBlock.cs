using System.Runtime.InteropServices;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block containing a sequence of pulses with arbitrary durations.
/// </summary>
public sealed class PulseSequenceBlock : TzxBlock<PulseSequenceHeader>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PulseSequenceBlock"/> class by reading from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public PulseSequenceBlock(Stream stream) : base(new PulseSequenceHeader(stream), stream)
    {
    }

    internal PulseSequenceBlock(byte[] headerData, byte[] data) : base(new PulseSequenceHeader(headerData), data)
    {
    }

    /// <summary>
    /// Gets the pulse durations in T-states.
    /// </summary>
    public ReadOnlySpan<ushort> Pulses => MemoryMarshal.Cast<byte, ushort>(AsSpan());

    /// <inheritdoc />
    public override string ToString() => $"{Header.Type}: {string.Join(", ", Pulses.ToArray())} T-States";
}