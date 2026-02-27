using MrKWatkins.OakIO.ZXSpectrum.Basic;
using static MrKWatkins.OakIO.ZXSpectrum.Basic.Basic;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// Represents a ZX Spectrum TAP tape file.
/// </summary>
public sealed class TapFile : ZXSpectrumTapeFile
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

    /// <summary>
    /// Concatenates two TAP files into a single file containing all blocks from both.
    /// </summary>
    /// <param name="file1">The first TAP file.</param>
    /// <param name="file2">The second TAP file.</param>
    /// <returns>A new <see cref="TapFile" /> containing all blocks from both files.</returns>
    [Pure]
    public static TapFile operator +(TapFile file1, TapFile file2) => new(file1.Blocks.Concat(file2.Blocks).ToArray());

    /// <summary>
    /// Gets the blocks in the TAP file.
    /// </summary>
    public IReadOnlyList<TapBlock> Blocks { get; }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <summary>
    /// Creates a TAP file containing a single code block.
    /// </summary>
    /// <param name="filename">The filename for the code block.</param>
    /// <param name="location">The memory location to load the code to.</param>
    /// <param name="data">The code data.</param>
    /// <returns>A new <see cref="TapFile" /> containing the code block.</returns>
    [Pure]
    public static TapFile CreateCode(string filename, ushort location, [InstantHandle] IEnumerable<byte> data)
    {
        var dataBlock = DataBlock.Create(data);
        var headerBlock = HeaderBlock.CreateCode(filename, location, (ushort)dataBlock.Length);
        return new TapFile(headerBlock, dataBlock);
    }

    /// <summary>
    /// Creates a TAP file containing multiple code blocks.
    /// </summary>
    /// <param name="filename">The filename for the code blocks.</param>
    /// <param name="codeBlocks">The code blocks, each with a memory location and data.</param>
    /// <returns>A new <see cref="TapFile" /> containing the code blocks.</returns>
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

    /// <summary>
    /// Creates a TAP file with a BASIC loader and code blocks.
    /// </summary>
    /// <param name="filename">The filename for the blocks.</param>
    /// <param name="codeBlocks">The code blocks, each with a memory location and data.</param>
    /// <returns>A new <see cref="TapFile" /> with a BASIC loader followed by code blocks.</returns>
    [Pure]
    public static TapFile CreateLoader(string filename, [InstantHandle] params IEnumerable<(ushort Location, byte[] Data)> codeBlocks) =>
        CreateLoader(filename, null, codeBlocks.ToArray());

    /// <summary>
    /// Creates a TAP file with a BASIC loader and code blocks, optionally specifying an entry point.
    /// </summary>
    /// <param name="filename">The filename for the blocks.</param>
    /// <param name="entryPoint">The entry point address for a RANDOMIZE USR call, or <c>null</c> for none.</param>
    /// <param name="codeBlocks">The code blocks, each with a memory location and data.</param>
    /// <returns>A new <see cref="TapFile" /> with a BASIC loader followed by code blocks.</returns>
    [Pure]
    public static TapFile CreateLoader(string filename, ushort? entryPoint, [InstantHandle] params IEnumerable<(ushort Location, byte[] Data)> codeBlocks) =>
        CreateLoader(filename, entryPoint, codeBlocks.ToArray());

    /// <summary>
    /// Creates a TAP file with a BASIC loader and code blocks.
    /// </summary>
    /// <param name="filename">The filename for the blocks.</param>
    /// <param name="codeBlocks">The code blocks, each with a memory location and data.</param>
    /// <returns>A new <see cref="TapFile" /> with a BASIC loader followed by code blocks.</returns>
    [Pure]
    public static TapFile CreateLoader(string filename, params IReadOnlyList<(ushort Location, byte[] Data)> codeBlocks) =>
        CreateLoader(filename, null, codeBlocks);

    /// <summary>
    /// Creates a TAP file with a BASIC loader and code blocks, optionally specifying an entry point.
    /// </summary>
    /// <param name="filename">The filename for the blocks.</param>
    /// <param name="entryPoint">The entry point address for a RANDOMIZE USR call, or <c>null</c> for none.</param>
    /// <param name="codeBlocks">The code blocks, each with a memory location and data.</param>
    /// <returns>A new <see cref="TapFile" /> with a BASIC loader followed by code blocks.</returns>
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

    /// <summary>
    /// Creates a TAP file with a TAP BAS style BASIC loader and a single code block.
    /// </summary>
    /// <param name="filename">The filename for the code block.</param>
    /// <param name="location">The memory location to load the code to.</param>
    /// <param name="data">The code data.</param>
    /// <param name="entryPoint">The entry point address for a RANDOMIZE USR call, or <c>null</c> for none.</param>
    /// <returns>A new <see cref="TapFile" /> with a BASIC loader followed by the code block.</returns>
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