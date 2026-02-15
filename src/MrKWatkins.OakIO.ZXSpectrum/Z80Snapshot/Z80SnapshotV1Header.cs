namespace MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Z80SnapshotV1Header : Header
{
    public Z80SnapshotV1Header()
        : this(new byte[30])
    {
    }

    internal Z80SnapshotV1Header(byte[] data)
        : base(data)
    {
        Registers = new Z80RegisterSnapshot(data, GetType() == typeof(Z80SnapshotV1Header));
    }

    public RegisterSnapshot Registers { get; }

    public ZXColour BorderColour
    {
        get => GetBits<ZXColour>(12, 1, 3);
        set => SetBits(12, value, 1, 3);
    }

    public bool DataIsCompressed
    {
        get => GetBit(12, 5);
        set => SetBit(12, 5, value);
    }

    public bool InterruptFlipFlop
    {
        get => GetBit(27, 0);
        set => SetBit(27, 0, value);
    }

    public bool IFF2
    {
        get => GetBit(28, 0);
        set => SetBit(28, 0, value);
    }

    public byte InterruptMode
    {
        get => GetBits(29, 0, 1);
        set => SetBits(29, value, 0, 1);
    }

    public VideoSynchronisation VideoSynchronisation
    {
        get => GetVideoSynchronisation(GetBits(29, 4, 5));
        set => SetByte(29, (byte)(GetVideoSynchronisationByte(value) | (Data[29] & 0b11001111)));
    }

    [Pure]
    private static VideoSynchronisation GetVideoSynchronisation(byte value) =>
        value switch
        {
            1 => VideoSynchronisation.High,
            3 => VideoSynchronisation.Low,
            _ => VideoSynchronisation.Normal
        };

    [Pure]
    private static byte GetVideoSynchronisationByte(VideoSynchronisation value) =>
        value switch
        {
            VideoSynchronisation.High => 0b00010000,
            VideoSynchronisation.Low => 0b00110000,
            _ => 0b00000000
        };

    public Joystick Joystick
    {
        get => GetBits<Joystick>(29, 6, 7);
        set => SetBits(29, value, 6, 7);
    }
}