namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// Header for a version 2 Z80 snapshot file.
/// </summary>
// https://worldofspectrum.org/faq/reference/z80format.htm
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Z80V2Header : Z80V1Header
{
    /// <summary>
    /// Initialises a new instance of the <see cref="Z80V2Header" /> class with default values.
    /// </summary>
    public Z80V2Header()
        : this(new byte[55])
    {
        ExtraLength = 23;
    }

    internal Z80V2Header(byte[] data)
        : base(data)
    {
    }

    /// <summary>
    /// Gets the length of the extra header data beyond the V1 header.
    /// </summary>
    public byte ExtraLength
    {
        get => GetByte(30);
        protected init => SetByte(30, value);
    }

    /// <summary>
    /// Gets the hardware mode of the snapshot.
    /// </summary>
#pragma warning disable CA1721
    public HardwareMode HardwareMode => GetHardwareMode(GetByte(34));
#pragma warning restore CA1721

    /// <summary>
    /// Maps a raw hardware mode byte value to a <see cref="HardwareMode" /> for V2 snapshots.
    /// </summary>
    /// <param name="hardwareMode">The raw hardware mode byte from the header.</param>
    /// <returns>The corresponding <see cref="HardwareMode" />.</returns>
    protected virtual HardwareMode GetHardwareMode(byte hardwareMode) =>
        hardwareMode switch
        {
            0 => HardwareMode.Spectrum48,
            1 => HardwareMode.Spectrum48,
            2 => HardwareMode.SamRam,
            3 => HardwareMode.Spectrum128,
            4 => HardwareMode.Spectrum128,
            _ => throw new NotSupportedException($"The {nameof(HardwareMode)} {hardwareMode} is not supported in v2 snapshots.")
        };
}