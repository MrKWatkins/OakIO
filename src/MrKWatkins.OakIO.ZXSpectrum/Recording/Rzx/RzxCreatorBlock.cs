using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// The creator information block (0x10) of an RZX file, describing the program that created it.
/// </summary>
public sealed class RzxCreatorBlock : RzxBlock
{
    private const int CreatorLength = 20;
    private const int MinimumDataLength = 24;

    /// <summary>
    /// Initialises a new instance of the <see cref="RzxCreatorBlock" /> class.
    /// </summary>
    /// <param name="creator">The creator's identification string, at most 20 ASCII characters.</param>
    /// <param name="majorVersion">The creator's major version number.</param>
    /// <param name="minorVersion">The creator's minor version number.</param>
    /// <param name="customData">Optional custom data appended to the block.</param>
    public RzxCreatorBlock(string creator, ushort majorVersion, ushort minorVersion, byte[]? customData = null)
        : this(new RzxBlockHeader(RzxBlockType.Creator, checked((uint)(MinimumDataLength + (customData?.Length ?? 0)))), CreateData(creator, majorVersion, minorVersion, customData))
    {
    }

    internal RzxCreatorBlock(RzxBlockHeader header, byte[] data)
        : base(header, data)
    {
        if (data.Length < MinimumDataLength)
        {
            throw new InvalidDataException("RZX creator block is too short.");
        }
    }

    /// <summary>
    /// Gets the creator's identification string, e.g. "RealSpectrum".
    /// </summary>
    public string Creator => GetString(0, CreatorLength);

    /// <summary>
    /// Gets the creator's major version number.
    /// </summary>
    public ushort MajorVersion => GetUInt16(20);

    /// <summary>
    /// Gets the creator's minor version number.
    /// </summary>
    public ushort MinorVersion => GetUInt16(22);

    /// <summary>
    /// Gets the creator's custom data. May be empty.
    /// </summary>
    [Pure]
    public ReadOnlySpan<byte> CustomData => AsReadOnlySpan(MinimumDataLength);

    [Pure]
    private static byte[] CreateData(string creator, ushort majorVersion, ushort minorVersion, byte[]? customData)
    {
        ArgumentNullException.ThrowIfNull(creator);

        var data = new byte[MinimumDataLength + (customData?.Length ?? 0)];
        WriteFixedAsciiString(data.AsSpan(0, CreatorLength), creator);
        data.SetUInt16(20, majorVersion);
        data.SetUInt16(22, minorVersion);
        customData?.CopyTo(data, MinimumDataLength);
        return data;
    }
}