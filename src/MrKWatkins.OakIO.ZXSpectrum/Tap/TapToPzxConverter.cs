using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Pzx;
using PzxDataBlock = MrKWatkins.OakIO.ZXSpectrum.Pzx.DataBlock;
using PzxPauseBlock = MrKWatkins.OakIO.ZXSpectrum.Pzx.PauseBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tap;

public sealed class TapToPzxConverter : IFormatConverter<TapFile, PzxFile>
{
    private const ushort PilotPulseLength = 2168;
    private const ushort HeaderPilotCount = 8063;
    private const ushort DataPilotCount = 3223;
    private const ushort Sync1Length = 667;
    private const ushort Sync2Length = 735;
    private const ushort ZeroBitPulseLength = 855;
    private const ushort OneBitPulseLength = 1710;
    private const ushort TailPulseLength = 945;
    private const uint PauseAfterBlockTStates = 3_500_000;

    public static readonly TapToPzxConverter Instance = new();

    private TapToPzxConverter()
    {
    }

    [Pure]
    public PzxFile Convert(TapFile source)
    {
        var blocks = new List<PzxBlock>();
        blocks.Add(BuildHeaderBlock());
        foreach (var block in source.Blocks)
        {
            blocks.AddRange(ConvertBlock(block));
        }
        return new PzxFile(blocks);
    }

    [Pure]
    private static PzxHeaderBlock BuildHeaderBlock()
    {
        using var stream = new MemoryStream();
        stream.WriteUInt32(2);
        stream.WriteByte(1);
        stream.WriteByte(0);
        stream.Position = 0;
        return new PzxHeaderBlock(stream);
    }

    [Pure]
    private static IEnumerable<PzxBlock> ConvertBlock(TapBlock block)
    {
        var blockData = BuildBlockData(block);
        var isHeader = block.Header.Type == TapBlockType.Header;
        yield return BuildPulsBlock(isHeader ? HeaderPilotCount : DataPilotCount);
        yield return BuildDataBlock(blockData);
        yield return BuildPausBlock();
    }

    [Pure]
    private static byte[] BuildBlockData(TapBlock block)
    {
        var data = new byte[1 + block.Length + 1];
        data[0] = (byte)block.Header.Type;
        block.CopyTo(data.AsSpan(1));
        data[^1] = block.Trailer.Checksum;
        return data;
    }

    [Pure]
    private static PulseSequenceBlock BuildPulsBlock(ushort pilotCount)
    {
        using var pulseStream = new MemoryStream();
        pulseStream.WriteWord((ushort)(0x8000 | pilotCount));
        pulseStream.WriteWord(PilotPulseLength);
        pulseStream.WriteWord(Sync1Length);
        pulseStream.WriteWord(Sync2Length);

        var size = (uint)pulseStream.Length;
        using var stream = new MemoryStream();
        stream.WriteUInt32(size);
        pulseStream.Position = 0;
        pulseStream.CopyTo(stream);
        stream.Position = 0;
        return new PulseSequenceBlock(stream);
    }

    [Pure]
    private static PzxDataBlock BuildDataBlock(byte[] blockData)
    {
        var sizeInBitsWithLevel = (uint)(blockData.Length * 8) | 0x80000000u;

        using var bodyStream = new MemoryStream();
        bodyStream.WriteWord(ZeroBitPulseLength);
        bodyStream.WriteWord(ZeroBitPulseLength);
        bodyStream.WriteWord(OneBitPulseLength);
        bodyStream.WriteWord(OneBitPulseLength);
        bodyStream.Write(blockData);

        var size = (uint)(8 + bodyStream.Length);
        using var stream = new MemoryStream();
        stream.WriteUInt32(size);
        stream.WriteUInt32(sizeInBitsWithLevel);
        stream.WriteWord(TailPulseLength);
        stream.WriteByte(2);
        stream.WriteByte(2);
        bodyStream.Position = 0;
        bodyStream.CopyTo(stream);
        stream.Position = 0;
        return new PzxDataBlock(stream);
    }

    [Pure]
    private static PzxPauseBlock BuildPausBlock()
    {
        using var stream = new MemoryStream();
        stream.WriteUInt32(4);
        stream.WriteUInt32(PauseAfterBlockTStates);
        stream.Position = 0;
        return new PzxPauseBlock(stream);
    }
}
