using MrKWatkins.OakIO.ZXSpectrum.Basic;
using static MrKWatkins.OakIO.ZXSpectrum.Basic.Basic;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

public sealed class TapFile : TapeFile
{
    internal TapFile(params TapBlock[] blocks)
        : base(TapFormat.Instance)
    {
        Blocks = blocks;
    }

    internal TapFile(IReadOnlyList<TapBlock> blocks)
        : base(TapFormat.Instance)
    {
        Blocks = blocks;
    }

    [Pure]
    public static TapFile operator +(TapFile file1, TapFile file2) => new(file1.Blocks.Concat(file2.Blocks).ToArray());

    public IReadOnlyList<TapBlock> Blocks { get; }

    public override bool TryLoadInto(Span<byte> memory)
    {
        for (var f = 0; f < Blocks.Count; f += 2)
        {
            if (Blocks[f] is not HeaderBlock header)
            {
                return false;
            }

            if (Blocks[f + 1] is not DataBlock data)
            {
                return false;
            }

            data.CopyTo(memory, header.Location);
        }

        return true;
    }

    public override void LoadInto(Span<byte> memory)
    {
        for (var f = 0; f < Blocks.Count; f += 2)
        {
            if (Blocks[f] is not HeaderBlock header)
            {
                throw new IOException("Missing header block when loading TAP file.");
            }

            if (Blocks[f + 1] is not DataBlock data)
            {
                throw new IOException("Missing data block after header when loading TAP file.");
            }

            data.CopyTo(memory, header.Location);
        }
    }

    [Pure]
    public static TapFile CreateCode(string filename, ushort location, [InstantHandle] IEnumerable<byte> data)
    {
        var dataBlock = DataBlock.Create(data);
        var headerBlock = HeaderBlock.CreateCode(filename, location, (ushort)dataBlock.Length);
        return new TapFile(headerBlock, dataBlock);
    }

    [Pure]
    public static TapFile CreateCode(string filename, [InstantHandle] params IEnumerable<(ushort Location, byte[] Data)> codeBlocks)
    {
        var blocks = new List<TapBlock>();
        foreach (var (location, data) in codeBlocks)
        {
            var dataBlock = DataBlock.Create(data);
            blocks.Add(HeaderBlock.CreateCode(filename, location, (ushort)dataBlock.Length));
            blocks.Add(dataBlock);
        }
        return new TapFile(blocks);
    }

    [Pure]
    public static TapFile CreateLoader(string filename, [InstantHandle] params IEnumerable<(ushort Location, byte[] Data)> codeBlocks) =>
        CreateLoader(filename, null, codeBlocks.ToArray());

    [Pure]
    public static TapFile CreateLoader(string filename, ushort? entryPoint, [InstantHandle] params IEnumerable<(ushort Location, byte[] Data)> codeBlocks) =>
        CreateLoader(filename, entryPoint, codeBlocks.ToArray());

    [Pure]
    public static TapFile CreateLoader(string filename, params IReadOnlyList<(ushort Location, byte[] Data)> codeBlocks) =>
        CreateLoader(filename, null, codeBlocks);

    [Pure]
    public static TapFile CreateLoader(string filename, ushort? entryPoint, params IReadOnlyList<(ushort Location, byte[] Data)> codeBlocks)
    {
        var blocks = new List<TapBlock>(2 + codeBlocks.Count * 2);

        var loader = BuildLoader(entryPoint, codeBlocks.Count);
        blocks.Add(HeaderBlock.CreateProgram(filename, (ushort)loader.Length, 10));
        blocks.Add(DataBlock.Create(loader));

        foreach (var (location, data) in codeBlocks)
        {
            blocks.Add(HeaderBlock.CreateCode(filename, location, (ushort)data.Length));
            blocks.Add(DataBlock.Create(data));
        }

        return new TapFile(blocks);
    }

    [Pure]
    private static byte[] BuildLoader(ushort? entryPoint, int numberOfCodeBlocks)
    {
        using var memoryStream = new MemoryStream();

        var basic = new BasicWriter(memoryStream);

        int lineNumber;
        for (lineNumber = 0; lineNumber <= numberOfCodeBlocks * 10; lineNumber += 10)
        {
            basic.WriteLine(lineNumber += 10, LOAD, "", CODE);
        }

        if (entryPoint.HasValue)
        {
            basic.WriteLine(lineNumber + 10, RANDOMIZE, USR, entryPoint.Value);
        }

        return memoryStream.ToArray();
    }

    [Pure]
    public static TapFile CreateTapBas(string filename, ushort location, byte[] data, ushort? entryPoint = null)
    {
        var loader = BuildTapBasLoader(location, entryPoint);

        return new TapFile(
            HeaderBlock.CreateProgram("loader", (ushort)loader.Length, 10),
            DataBlock.Create(loader),
            HeaderBlock.CreateCode(filename, location, (ushort)data.Length),
            DataBlock.Create(data));
    }

    [Pure]
    private static byte[] BuildTapBasLoader(ushort location, ushort? entryPoint)
    {
        using var memoryStream = new MemoryStream();
        var basic = new BasicWriter(memoryStream);

        basic.WriteLine(10, CLEAR, location - 1);
        basic.WriteLine(20, POKE, 23610, ',', 255);
        basic.WriteLine(30, LOAD, "", CODE);

        if (entryPoint.HasValue)
        {
            basic.WriteLine(40, RANDOMIZE, USR, entryPoint.Value);
        }

        return memoryStream.ToArray();
    }
}