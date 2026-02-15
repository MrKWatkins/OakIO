using System.Runtime.InteropServices;

namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public sealed class PulseSequenceBlock : PzxBlock<PulseSequenceHeader>
{
    public PulseSequenceBlock(Stream stream) : base(new PulseSequenceHeader(stream), stream)
    {
        Pulses = ReadPulses();
    }

    public IReadOnlyList<Pulse> Pulses { get; }

    public override string ToString() => $"{Header.Type}: {string.Join(", ", Pulses)}";

    private List<Pulse> ReadPulses()
    {
        var pulses = new List<Pulse>();
        var enumerator = MemoryMarshal.Cast<byte, ushort>(AsSpan()).GetEnumerator();
        while (true)
        {
            if (!enumerator.MoveNext())
            {
                break;
            }

            ushort count = 1;
            var duration = enumerator.Current;
            if (duration >= 0x8000)
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