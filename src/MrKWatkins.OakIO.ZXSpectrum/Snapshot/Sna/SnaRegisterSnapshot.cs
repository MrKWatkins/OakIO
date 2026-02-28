using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

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
        get => GetUInt16(21);
        set => SetUInt16(21, value);
    }

    public override ushort BC
    {
        get => GetUInt16(13);
        set => SetUInt16(13, value);
    }

    public override ushort DE
    {
        get => GetUInt16(11);
        set => SetUInt16(11, value);
    }

    public override ushort HL
    {
        get => GetUInt16(9);
        set => SetUInt16(9, value);
    }

    public override ushort IX
    {
        get => GetUInt16(17);
        set => SetUInt16(17, value);
    }

    public override ushort IY
    {
        get => GetUInt16(15);
        set => SetUInt16(15, value);
    }

    public override ushort PC
    {
        get => footerData.GetUInt16(0);
        set => footerData.SetUInt16(0, value);
    }

    public override ushort SP
    {
        get => GetUInt16(23);
        set => SetUInt16(23, value);
    }

    public override ushort IR
    {
        get => Endian.Little.ToUInt16(GetByte(0), GetByte(20));
        set
        {
            var (r, i) = value.ToBytes();
            SetByte(0, i);
            SetByte(20, r);
        }
    }

    public override ShadowRegisterSnapshot Shadow { get; }
}