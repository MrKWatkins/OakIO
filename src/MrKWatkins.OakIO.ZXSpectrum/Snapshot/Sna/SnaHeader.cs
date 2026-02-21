namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class SnaHeader : Header
{
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

    public RegisterSnapshot Registers { get; }

    public bool IFF2
    {
        get => GetBit(19, 2);
        set => SetBit(19, 2, value);
    }

    public byte InterruptMode
    {
        get => GetByte(25);
        set => SetByte(25, value);
    }

    public ZXColour BorderColour
    {
        get => GetByte<ZXColour>(26);
        set => SetByte(26, value);
    }
}