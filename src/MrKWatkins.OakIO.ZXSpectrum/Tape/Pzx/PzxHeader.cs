namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// The header for a <see cref="PzxHeaderBlock" />.
/// </summary>
public sealed class PzxHeader : PzxBlockHeader
{
    internal PzxHeader(byte[] data)
        : base(PzxBlockType.Header, data)
    {
    }

    /// <summary>
    /// Gets the major version number of the PZX format.
    /// </summary>
    public byte MajorVersionNumber => GetByte(StartIndex);

    /// <summary>
    /// Gets the minor version number of the PZX format.
    /// </summary>
    public byte MinorVersionNumber => GetByte(StartIndex + 1);
}