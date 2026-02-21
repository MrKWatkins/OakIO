using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.SnaSnapshot;

internal sealed class SnaRegisterSnapshot : RegisterSnapshot
{
    private readonly byte[] footerData;

    internal SnaRegisterSnapshot(byte[] headerData, byte[] footerData)
        : base(headerData)
    {
        this.footerData = footerData;
        Shadow = new SnaShadowRegisterSnapshot(headerData);
    }

    public override ushort AF
    {
        get => GetWord(21);
        set => SetWord(21, value);
    }

    public override ushort BC
    {
        get => GetWord(13);
        set => SetWord(13, value);
    }

    public override ushort DE
    {
        get => GetWord(11);
        set => SetWord(11, value);
    }

    public override ushort HL
    {
        get => GetWord(9);
        set => SetWord(9, value);
    }

    public override ushort IX
    {
        get => GetWord(17);
        set => SetWord(17, value);
    }

    public override ushort IY
    {
        get => GetWord(15);
        set => SetWord(15, value);
    }

    public override ushort PC
    {
        get => footerData.GetWord(0);
        set => footerData.SetWord(0, value);
    }

    public override ushort SP
    {
        get => GetWord(23);
        set => SetWord(23, value);
    }

    public override ushort IR
    {
        get => Endian.Little.ToWord(GetByte(0), GetByte(20));
        set
        {
            var (r, i) = value.ToBytes();
            SetByte(0, i);
            SetByte(20, r);
        }
    }

    public override ShadowRegisterSnapshot Shadow { get; }
}