namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class TzxHeader(byte[] data) : Header(data)
{
    internal const int ExpectedLength = 10;

    public TzxHeader(byte majorVersion, byte minorVersion)
        : this("ZXTape!\x1A\x00\x00"u8.ToArray())
    {
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
    }

    public byte MajorVersion
    {
        get => GetByte(8);
        private init => SetByte(8, value);
    }

    public byte MinorVersion
    {
        get => GetByte(9);
        private init => SetByte(9, value);
    }

    public bool IsValid => Data.Take(8).SequenceEqual("ZXTape!\x1A"u8.ToArray());
}