namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// Header for a memory page in a V2 or V3 Z80 snapshot file.
/// </summary>
public sealed class PageHeader : Header
{
    internal PageHeader(HardwareMode hardwareMode, byte[] header) : base(header)
    {
        HardwareMode = hardwareMode;
    }

    internal PageHeader(HardwareMode hardwareMode, ushort compressedLength, byte pageNumber)
        : this(hardwareMode, new byte[3])
    {
        SetUInt16(0, compressedLength);
        SetByte(2, pageNumber);
    }

    /// <summary>
    /// Gets the hardware mode associated with this page.
    /// </summary>
    public HardwareMode HardwareMode { get; }

    /// <summary>
    /// Gets the compressed length of the page data, or 0xFFFF if uncompressed.
    /// </summary>
    public ushort CompressedLength => GetUInt16(0);

    /// <summary>
    /// Gets the page number.
    /// </summary>
    public byte PageNumber => GetByte(2);

    /// <summary>
    /// Gets the memory location this page maps to.
    /// </summary>
    public ushort Location => GetLocation(HardwareMode, PageNumber);

    [Pure]
    internal static ushort GetLocation(HardwareMode hardwareMode, byte pageNumber) =>
        hardwareMode switch
        {
            HardwareMode.Spectrum48 => GetSpectrum48DataLocation(pageNumber),
            HardwareMode.Spectrum128 => GetSpectrum128DataLocation(pageNumber),
            _ => throw new NotSupportedException($"The {nameof(hardwareMode)} {hardwareMode} is not supported.")
        };

    [Pure]
    private static ushort GetSpectrum48DataLocation(byte pageNumber) =>
        pageNumber switch
        {
            4 => 0x8000,
            5 => 0xC000,
            8 => 0x4000,
            _ => throw new NotSupportedException($"Page number {pageNumber} is not supported in {nameof(HardwareMode.Spectrum48)} mode.")
        };

    [Pure]
    private static ushort GetSpectrum128DataLocation(byte pageNumber) =>
        pageNumber switch
        {
            5 => 0x4000,
            2 => 0x8000,
            0 => 0xC000,
            _ => throw new NotSupportedException($"Page number {pageNumber} is not supported in {nameof(HardwareMode.Spectrum128)} mode.")
        };
}