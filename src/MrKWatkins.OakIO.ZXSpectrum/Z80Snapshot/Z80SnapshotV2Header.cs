namespace MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

// https://worldofspectrum.org/faq/reference/z80format.htm
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Z80SnapshotV2Header : Z80SnapshotV1Header
{
    public Z80SnapshotV2Header()
        : this(new byte[55])
    {
        ExtraLength = 23;
    }

    internal Z80SnapshotV2Header(byte[] data)
        : base(data)
    {
    }

    public byte ExtraLength
    {
        get => GetByte(30);
        protected init => SetByte(30, value);
    }

#pragma warning disable CA1721
    public HardwareMode HardwareMode => GetHardwareMode(GetByte(34));
#pragma warning restore CA1721

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