using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Tzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tap;

public sealed class TapToTzxConverter : IFormatConverter<TapFile, TzxFile>
{
    public static readonly TapToTzxConverter Instance = new();

    private TapToTzxConverter()
    {
    }

    [Pure]
    public TzxFile Convert(TapFile source)
    {
        var header = new TzxHeader(1, 20);
        var blocks = source.Blocks.Select(ConvertBlock).ToList();
        return new TzxFile(header, blocks);
    }

    [Pure]
    private static StandardSpeedDataBlock ConvertBlock(TapBlock block)
    {
        var data = BuildBlockData(block);
        using var stream = new MemoryStream();
        stream.WriteWord(1000);
        stream.WriteWord((ushort)data.Length);
        stream.Write(data);
        stream.Position = 0;
        return new StandardSpeedDataBlock(stream);
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
