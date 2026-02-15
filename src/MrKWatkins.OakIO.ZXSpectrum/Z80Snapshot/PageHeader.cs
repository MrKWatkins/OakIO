namespace MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

public sealed class PageHeader : Header
{
    internal PageHeader(HardwareMode hardwareMode, byte[] header) : base(header)
    {
        HardwareMode = hardwareMode;
    }

    internal PageHeader(HardwareMode hardwareMode, ushort compressedLength, byte pageNumber)
        : this(hardwareMode, new byte[3])
    {
        SetWord(0, compressedLength);
        SetByte(2, pageNumber);
    }

    public HardwareMode HardwareMode { get; }

    public ushort CompressedLength => GetWord(0);

    public byte PageNumber => GetByte(2);

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