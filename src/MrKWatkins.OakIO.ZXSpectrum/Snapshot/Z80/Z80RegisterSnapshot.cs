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
        get => GetWord(0, Endian.Big);
        set => SetWord(0, value, Endian.Big);
    }

    public override ushort BC
    {
        get => GetWord(2);
        set => SetWord(2, value);
    }

    public override ushort DE
    {
        get => GetWord(13);
        set => SetWord(13, value);
    }

    public override ushort HL
    {
        get => GetWord(4);
        set => SetWord(4, value);
    }

    public override ushort IX
    {
        get => GetWord(25);
        set => SetWord(25, value);
    }

    public override ushort IY
    {
        get => GetWord(23);
        set => SetWord(23, value);
    }

    public override ushort PC
    {
        get => GetWord(v1 ? 6 : 32);
        set => SetWord(v1 ? 6 : 32, value);
    }

    public override ushort SP
    {
        get => GetWord(8);
        set => SetWord(8, value);
    }

    public override ushort IR
    {
        get
        {
            var i = GetByte(10);
            var r = GetBits(11, 0, 6) | (GetBits(12, 0, 0) << 7);
            return Endian.Little.ToWord(i, (byte)r);
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