using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

internal sealed class Z80ShadowRegisterSnapshot : ShadowRegisterSnapshot
{
    internal Z80ShadowRegisterSnapshot(byte[] data)
        : base(data)
    {
    }

    public override ushort AF
    {
        get => GetWord(21, Endian.Big);
        set => SetWord(21, value, Endian.Big);
    }

    public override ushort BC
    {
        get => GetWord(15);
        set => SetWord(15, value);
    }

    public override ushort DE
    {
        get => GetWord(17);
        set => SetWord(17, value);
    }

    public override ushort HL
    {
        get => GetWord(19);
        set => SetWord(19, value);
    }
}