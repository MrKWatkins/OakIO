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
        var bytes = new byte[6];
        bytes.SetUInt32(0, 2);
        bytes[4] = 1;
        bytes[5] = 0;
        return new PzxHeaderBlock(new MemoryStream(bytes));
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
        const int pulseDataLength = 8;
        var bytes = new byte[4 + pulseDataLength];
        bytes.SetUInt32(0, pulseDataLength);
        bytes.SetWord(4, (ushort)(0x8000 | pilotCount));
        bytes.SetWord(6, PilotPulseLength);
        bytes.SetWord(8, Sync1Length);
        bytes.SetWord(10, Sync2Length);
        return new PulseSequenceBlock(new MemoryStream(bytes));
    }

    [Pure]
    private static PzxDataBlock BuildDataBlock(byte[] blockData)
    {
        var sizeInBitsWithLevel = (uint)(blockData.Length * 8) | 0x80000000u;
        var bodyLength = 4 * 2 + blockData.Length;
        var bytes = new byte[4 + 8 + bodyLength];
        bytes.SetUInt32(0, (uint)(8 + bodyLength));
        bytes.SetUInt32(4, sizeInBitsWithLevel);
        bytes.SetWord(8, TailPulseLength);
        bytes[10] = 2;
        bytes[11] = 2;
        bytes.SetWord(12, ZeroBitPulseLength);
        bytes.SetWord(14, ZeroBitPulseLength);
        bytes.SetWord(16, OneBitPulseLength);
        bytes.SetWord(18, OneBitPulseLength);
        blockData.CopyTo(bytes, 20);
        return new PzxDataBlock(new MemoryStream(bytes));
    }

    [Pure]
    private static PzxPauseBlock BuildPausBlock()
    {
        var bytes = new byte[8];
        bytes.SetUInt32(0, 4);
        bytes.SetUInt32(4, PauseAfterBlockTStates);
        return new PzxPauseBlock(new MemoryStream(bytes));
    }
}
