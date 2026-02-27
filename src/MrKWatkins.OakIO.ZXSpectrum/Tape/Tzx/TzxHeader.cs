namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// The header of a TZX tape file containing format identification and version information.
/// </summary>
/// <param name="data">The raw header bytes.</param>
public sealed class TzxHeader(byte[] data) : Header(data)
{
    internal const int ExpectedLength = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="TzxHeader"/> class with the specified version.
    /// </summary>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The minor version number.</param>
    public TzxHeader(byte majorVersion, byte minorVersion)
        : this("ZXTape!\x1A\x00\x00"u8.ToArray())
    {
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
    }

    /// <summary>
    /// Gets the major version number of the TZX format.
    /// </summary>
    public byte MajorVersion
    {
        get => GetByte(8);
        private init => SetByte(8, value);
    }

    /// <summary>
    /// Gets the minor version number of the TZX format.
    /// </summary>
    public byte MinorVersion
    {
        get => GetByte(9);
        private init => SetByte(9, value);
    }

    /// <summary>
    /// Gets a value indicating whether this header contains a valid TZX signature.
    /// </summary>
    public bool IsValid => Data.Take(8).SequenceEqual("ZXTape!\x1A"u8.ToArray());
}