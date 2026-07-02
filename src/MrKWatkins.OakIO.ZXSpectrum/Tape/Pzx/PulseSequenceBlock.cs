using System.Runtime.InteropServices;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// A PZX block containing a raw pulse sequence.
/// </summary>
public sealed class PulseSequenceBlock : PzxBlock<PulseSequenceHeader>
{
    internal PulseSequenceBlock(byte[] headerData, byte[] bodyData) : base(new PulseSequenceHeader(headerData), bodyData)
    {
        Pulses = ReadPulses();
    }

    /// <summary>
    /// Gets the pulses in this block.
    /// </summary>
    public IReadOnlyList<Pulse> Pulses { get; }

    /// <inheritdoc />
    [Pure]
    public override string ToString() => $"{Header.Type}: {string.Join(", ", Pulses)}";

    [Pure]
    private List<Pulse> ReadPulses()
    {
        var pulses = new List<Pulse>();
        using var enumerator = MemoryMarshal.Cast<byte, ushort>(AsSpan()).GetEnumerator();
        while (true)
        {
            if (!enumerator.MoveNext())
            {
                break;
            }

            ushort count = 1;
            uint duration = enumerator.Current;
            // A first word > 0x8000 is a repeat count; a duration of exactly 0x8000 is an extended (31-bit) duration.
            if (duration > 0x8000)
            {
                count = (ushort)(duration & 0x7FFF);
                if (!enumerator.MoveNext())
                {
                    throw new IOException("Unexpected end of data in pulse block.");
                }
                duration = enumerator.Current;
            }

            if (duration >= 0x8000)
            {
                duration &= 0x7FFF;
                duration <<= 16;
                if (!enumerator.MoveNext())
                {
                    throw new IOException("Unexpected end of data in pulse block.");
                }
                duration |= enumerator.Current;
            }

            pulses.Add(new Pulse(count, duration));
        }
        return pulses;
    }
}