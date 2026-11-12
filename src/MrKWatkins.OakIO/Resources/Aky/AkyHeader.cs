using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>The native AKY version, channel count and PSG clock header.</summary>
public sealed class AkyHeader : Header
{
    internal AkyHeader(byte[] bytes) : base(bytes)
    {
    }

    /// <summary>Gets the low four-bit file format version.</summary>
    public int Version => Data[0] & 15;

    /// <summary>Gets whether multi-byte values use little-endian byte order.</summary>
    public bool IsLittleEndian => (Data[0] & 128) != 0;

    /// <summary>Gets the SID period profile, independently of whether SID commands occur.</summary>
    // Profile and padding positions follow the exporter rather than the older format notes.
    // https://bitbucket.org/JulienNevo/arkostracker3/src/14d975bdf0787cde4a5fa9425abe793e4a3a3536/source/export/aky/AkyExporter.cpp
    public AkySidProfile SidProfile => (AkySidProfile)((Data[0] >> 4) & 3);

    /// <summary>Gets the encoded channel count.</summary>
    public int ChannelCount => Data[1];

    /// <summary>Gets the number of three-channel PSG chips.</summary>
    public int ChipCount => (ChannelCount + 2) / 3;

    /// <summary>Reads one chip's clock from the native header.</summary>
    /// <param name="chip">The zero-based chip index.</param>
    /// <returns>The clock in hertz.</returns>
    [Pure]
    public uint GetClock(int chip)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(chip);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(chip, ChipCount);
        return GetUInt32(2 + (chip * 4), IsLittleEndian ? Endian.Little : Endian.Big);
    }
}