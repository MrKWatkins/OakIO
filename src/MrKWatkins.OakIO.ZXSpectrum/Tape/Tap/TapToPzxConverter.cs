using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;
using PzxDataBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx.DataBlock;
using PzxPauseBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx.PauseBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// Converts TAP files to PZX format.
/// </summary>
public sealed class TapToPzxConverter : IOFileConverter<TapFile, PzxFile>
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

    internal TapToPzxConverter()
        : base(TapFormat.Instance, PzxFormat.Instance)
    {
    }

    /// <inheritdoc />
    [Pure]
    public override PzxFile Convert(TapFile source)
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
        var headerData = new byte[6];
        headerData.SetUInt32(0, 2);
        headerData[4] = 1;
        headerData[5] = 0;
        return new PzxHeaderBlock(headerData);
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
        var headerData = new byte[4];
        headerData.SetUInt32(0, 8);
        var bodyData = new byte[8];
        bodyData.SetWord(0, (ushort)(0x8000 | pilotCount));
        bodyData.SetWord(2, PilotPulseLength);
        bodyData.SetWord(4, Sync1Length);
        bodyData.SetWord(6, Sync2Length);
        return new PulseSequenceBlock(headerData, bodyData);
    }

    [Pure]
    private static PzxDataBlock BuildDataBlock(byte[] blockData)
    {
        var sizeInBitsWithLevel = (uint)(blockData.Length * 8) | 0x80000000u;
        var headerData = new byte[12];
        headerData.SetUInt32(0, (uint)(8 + 8 + blockData.Length));
        headerData.SetUInt32(4, sizeInBitsWithLevel);
        headerData.SetWord(8, TailPulseLength);
        headerData[10] = 2;
        headerData[11] = 2;
        var bodyData = new byte[8 + blockData.Length];
        bodyData.SetWord(0, ZeroBitPulseLength);
        bodyData.SetWord(2, ZeroBitPulseLength);
        bodyData.SetWord(4, OneBitPulseLength);
        bodyData.SetWord(6, OneBitPulseLength);
        blockData.CopyTo(bodyData, 8);
        return new PzxDataBlock(headerData, bodyData);
    }

    [Pure]
    private static PzxPauseBlock BuildPausBlock()
    {
        var headerData = new byte[8];
        headerData.SetUInt32(0, 4);
        headerData.SetUInt32(4, PauseAfterBlockTStates);
        return new PzxPauseBlock(headerData);
    }
}