namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

/// <summary>
/// The header of an SNA snapshot file.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class SnaHeader : Header
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SnaHeader" /> class with default values.
    /// </summary>
    public SnaHeader()
        : this(new byte[27])
    {
    }

    internal SnaHeader(byte[] data)
        : base(data)
    {
        Registers = new SnaRegisterSnapshot(data, new byte[2]);
    }

    internal SnaHeader(byte[] data, byte[] footerData)
        : base(data)
    {
        Registers = new SnaRegisterSnapshot(data, footerData);
    }

    /// <summary>
    /// Gets the CPU register snapshot.
    /// </summary>
    public RegisterSnapshot Registers { get; }

    /// <summary>
    /// Gets or sets a value indicating whether interrupt flip-flop 2 is set.
    /// </summary>
    public bool IFF2
    {
        get => GetBit(19, 2);
        set => SetBit(19, 2, value);
    }

    /// <summary>
    /// Gets or sets the interrupt mode.
    /// </summary>
    public byte InterruptMode
    {
        get => GetByte(25);
        set => SetByte(25, value);
    }

    /// <summary>
    /// Gets or sets the border colour.
    /// </summary>
    public ZXColour BorderColour
    {
        get => GetByte<ZXColour>(26);
        set => SetByte(26, value);
    }
}