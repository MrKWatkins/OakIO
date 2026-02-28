using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

internal sealed class Z80RegisterSnapshot : RegisterSnapshot
{
    private readonly bool v1;

    internal Z80RegisterSnapshot(byte[] data, bool v1)
        : base(data)
    {
        this.v1 = v1;
        Shadow = new Z80ShadowRegisterSnapshot(data);
    }

    public override ushort AF
    {
        get => GetUInt16(0, Endian.Big);
        set => SetUInt16(0, value, Endian.Big);
    }

    public override ushort BC
    {
        get => GetUInt16(2);
        set => SetUInt16(2, value);
    }

    public override ushort DE
    {
        get => GetUInt16(13);
        set => SetUInt16(13, value);
    }

    public override ushort HL
    {
        get => GetUInt16(4);
        set => SetUInt16(4, value);
    }

    public override ushort IX
    {
        get => GetUInt16(25);
        set => SetUInt16(25, value);
    }

    public override ushort IY
    {
        get => GetUInt16(23);
        set => SetUInt16(23, value);
    }

    public override ushort PC
    {
        get => GetUInt16(v1 ? 6 : 32);
        set => SetUInt16(v1 ? 6 : 32, value);
    }

    public override ushort SP
    {
        get => GetUInt16(8);
        set => SetUInt16(8, value);
    }

    public override ushort IR
    {
        get
        {
            var i = GetByte(10);
            var r = GetBits(11, 0, 6) | (GetBits(12, 0, 0) << 7);
            return Endian.Little.ToUInt16(i, (byte)r);
        }
        set
        {
            var (r, i) = value.ToBytes();
            SetByte(10, i);
            SetBits(11, r, 0, 6);
            SetBit(12, 0, r.GetBit(7));
        }
    }

    public override ShadowRegisterSnapshot Shadow { get; }
}