namespace MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

// https://worldofspectrum.org/faq/reference/z80format.htm
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class Z80SnapshotV3Header : Z80SnapshotV2Header
{
    public Z80SnapshotV3Header()
        : this(new byte[87])
    {
        ExtraLength = 55;
    }

    internal Z80SnapshotV3Header(byte[] data)
        : base(data)
    {
    }

    protected override HardwareMode GetHardwareMode(byte hardwareMode) =>
        hardwareMode switch
        {
            0 => HardwareMode.Spectrum48,
            1 => HardwareMode.Spectrum48,
            2 => HardwareMode.SamRam,
            3 => HardwareMode.Spectrum48,
            4 => HardwareMode.Spectrum128,
            5 => HardwareMode.Spectrum128,
            6 => HardwareMode.Spectrum128,
            _ => throw new NotSupportedException($"The {nameof(HardwareMode)} {hardwareMode} is not supported in v3 snapshots.")
        };
}