namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

public abstract class TapBlock : Block<TapHeader, TapTrailer>
{
    private protected TapBlock(TapHeader header, TapTrailer trailer, byte[] data)
        : base(header, trailer, data)
    {
    }

    [Pure]
    protected static (byte Checksum, byte[] Data) CalculateChecksum(TapBlockType type, [InstantHandle] IEnumerable<byte> data)
    {
        var result = new List<byte>();
        var checksum = (byte)type;
        foreach (var @byte in data)
        {
            checksum ^= @byte;
            result.Add(@byte);
        }
        return (checksum, result.ToArray());
    }

    public byte Checksum => Data.Aggregate((byte)Header.Type, (current, b) => (byte)(current ^ b));
}

public abstract class TapBlock<THeader> : TapBlock
    where THeader : TapHeader
{
    private protected TapBlock(THeader header, TapTrailer trailer, byte[] data)
        : base(header, trailer, data)
    {
    }

    public new THeader Header => (THeader)base.Header;
}