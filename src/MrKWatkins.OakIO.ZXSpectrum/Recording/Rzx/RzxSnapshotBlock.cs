using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// A snapshot block (0x30) of an RZX file, containing the machine state at the start of a recording.
/// </summary>
public sealed class RzxSnapshotBlock : RzxBlock
{
    private const int ExtensionLength = 4;
    private const int MinimumDataLength = 12;

    private readonly byte[] snapshotData;

    /// <summary>
    /// Initializes a new instance of the <see cref="RzxSnapshotBlock" /> class containing an uncompressed, embedded snapshot.
    /// </summary>
    /// <param name="extension">The snapshot filename extension, e.g. "Z80" or "SNA".</param>
    /// <param name="data">The uncompressed snapshot data.</param>
    /// <param name="flags">The snapshot flags. Only <see cref="RzxSnapshotFlags.None" /> is supported for writing.</param>
    /// <param name="uncompressedLength">The uncompressed snapshot length. Defaults to the length of <paramref name="data" />.</param>
    public RzxSnapshotBlock(string extension, byte[] data, RzxSnapshotFlags flags = RzxSnapshotFlags.None, uint? uncompressedLength = null)
        : this(new RzxBlockHeader(RzxBlockType.Snapshot, GetDataLength(data)), CreateData(extension, data, flags, uncompressedLength))
    {
    }

    internal RzxSnapshotBlock(RzxBlockHeader header, byte[] data)
        : base(header, data)
    {
        if (data.Length < MinimumDataLength)
        {
            throw new InvalidDataException("RZX snapshot block is too short.");
        }

        if (Flags.HasFlag(RzxSnapshotFlags.ExternalData))
        {
            throw new NotSupportedException("External RZX snapshot blocks are not supported.");
        }

        var rawData = AsReadOnlySpan(MinimumDataLength).ToArray();
        snapshotData = Flags.HasFlag(RzxSnapshotFlags.Compressed)
            ? ZLib.Decompress(rawData, checked((int)UncompressedLength))
            : rawData;

        if (UncompressedLength != snapshotData.Length)
        {
            throw new InvalidDataException("Uncompressed RZX snapshot length does not match the data length.");
        }
    }

    /// <summary>
    /// Gets the snapshot flags.
    /// </summary>
    public RzxSnapshotFlags Flags => (RzxSnapshotFlags)GetUInt32(0);

    /// <summary>
    /// Gets the snapshot filename extension, e.g. "Z80" or "SNA".
    /// </summary>
    public string Extension => GetString(4, ExtensionLength);

    /// <summary>
    /// Gets the length of the uncompressed snapshot data.
    /// </summary>
    public uint UncompressedLength => GetUInt32(8);

    /// <summary>
    /// Gets the uncompressed snapshot data, decompressed if necessary.
    /// </summary>
    public IReadOnlyList<byte> SnapshotData => snapshotData;

    [Pure]
    private static uint GetDataLength(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return checked((uint)(MinimumDataLength + data.Length));
    }

    [Pure]
    private static byte[] CreateData(string extension, byte[] data, RzxSnapshotFlags flags, uint? uncompressedLength)
    {
        ArgumentNullException.ThrowIfNull(extension);
        ArgumentNullException.ThrowIfNull(data);
        if (flags != RzxSnapshotFlags.None)
        {
            throw new InvalidDataException("Only uncompressed embedded RZX snapshot blocks can be written.");
        }

        var body = new byte[MinimumDataLength + data.Length];
        body.SetUInt32(0, (uint)flags);
        WriteFixedAsciiString(body.AsSpan(4, ExtensionLength), extension);
        body.SetUInt32(8, uncompressedLength ?? checked((uint)data.Length));
        data.CopyTo(body, MinimumDataLength);
        return body;
    }
}