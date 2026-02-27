namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// Header for a version 1 Z80 snapshot file.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Z80V1Header : Header
{
    /// <summary>
    /// Initialises a new instance of the <see cref="Z80V1Header" /> class with default values.
    /// </summary>
    public Z80V1Header()
        : this(new byte[30])
    {
    }

    internal Z80V1Header(byte[] data)
        : base(data)
    {
        Registers = new Z80RegisterSnapshot(data, GetType() == typeof(Z80V1Header));
    }

    /// <summary>
    /// Gets the register snapshot from this header.
    /// </summary>
    public RegisterSnapshot Registers { get; }

    /// <summary>
    /// Gets or sets the border colour.
    /// </summary>
    public ZXColour BorderColour
    {
        get => GetBits<ZXColour>(12, 1, 3);
        set => SetBits(12, value, 1, 3);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the data in a V1 file is compressed.
    /// </summary>
    public bool DataIsCompressed
    {
        get => GetBit(12, 5);
        set => SetBit(12, 5, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the interrupt flip-flop is set.
    /// </summary>
    public bool InterruptFlipFlop
    {
        get => GetBit(27, 0);
        set => SetBit(27, 0, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether interrupt flip-flop 2 (IFF2) is set.
    /// </summary>
    public bool IFF2
    {
        get => GetBit(28, 0);
        set => SetBit(28, 0, value);
    }

    /// <summary>
    /// Gets or sets the Z80 interrupt mode (0, 1, or 2).
    /// </summary>
    public byte InterruptMode
    {
        get => GetBits(29, 0, 1);
        set => SetBits(29, value, 0, 1);
    }

    /// <summary>
    /// Gets or sets the video synchronisation mode.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the joystick type.
    /// </summary>
    public Joystick Joystick
    {
        get => GetBits<Joystick>(29, 6, 7);
        set => SetBits(29, value, 6, 7);
    }
}