namespace MrKWatkins.OakIO.ZXSpectrum.Nex;

public sealed class NexFile : SnapshotFile
{
    private readonly NexRegisterSnapshot registers;

    internal NexFile(NexHeader header, byte[]? palette, IReadOnlyList<NexScreen> screens, byte[]? copperCode, IReadOnlyList<NexBank> banks)
        : base(NexFormat.Instance)
    {
        Header = header;
        Palette = palette;
        Screens = screens;
        CopperCode = copperCode;
        Banks = banks;
        registers = new NexRegisterSnapshot(header);
    }

    public NexHeader Header { get; }

    public byte[]? Palette { get; }

    public IReadOnlyList<NexScreen> Screens { get; }

    public byte[]? CopperCode { get; }

    public IReadOnlyList<NexBank> Banks { get; }

    public override RegisterSnapshot Registers => registers;
}