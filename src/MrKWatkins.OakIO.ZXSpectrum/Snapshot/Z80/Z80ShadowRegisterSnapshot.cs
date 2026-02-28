using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

internal sealed class Z80ShadowRegisterSnapshot : ShadowRegisterSnapshot
{
    internal Z80ShadowRegisterSnapshot(byte[] data)
        : base(data)
    {
    }

    public override ushort AF
    {
        get => GetUInt16(21, Endian.Big);
        set => SetUInt16(21, value, Endian.Big);
    }

    public override ushort BC
    {
        get => GetUInt16(15);
        set => SetUInt16(15, value);
    }

    public override ushort DE
    {
        get => GetUInt16(17);
        set => SetUInt16(17, value);
    }

    public override ushort HL
    {
        get => GetUInt16(19);
        set => SetUInt16(19, value);
    }
}