using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

public sealed class TapToTzxConverter : IOFileConverter<TapFile, TzxFile>
{
    internal TapToTzxConverter()
        : base(TapFormat.Instance, TzxFormat.Instance)
    {
    }

    [Pure]
    public override TzxFile Convert(TapFile source)
    {
        var header = new TzxHeader(1, 20);
        var blocks = source.Blocks.Select(ConvertBlock).ToList();
        return new TzxFile(header, blocks);
    }

    [Pure]
    private static StandardSpeedDataBlock ConvertBlock(TapBlock block)
    {
        var data = BuildBlockData(block);
        var headerData = new byte[4];
        headerData.SetWord(0, 1000);
        headerData.SetWord(2, (ushort)data.Length);
        return new StandardSpeedDataBlock(headerData, data);
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
}