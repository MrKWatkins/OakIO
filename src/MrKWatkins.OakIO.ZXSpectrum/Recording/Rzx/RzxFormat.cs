using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// The RZX input recording file format.
/// </summary>
// https://worldofspectrum.net/RZXformat.html
public sealed class RzxFormat : ZXSpectrumRecordingFormat<RzxFile>
{
    /// <summary>
    /// The singleton instance of the RZX format.
    /// </summary>
    public static readonly RzxFormat Instance = new();

    private RzxFormat()
        : base("RZX Input Recording", "rzx")
    {
    }

    /// <inheritdoc />
    [MustUseReturnValue]
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader)
    {
        var header = new RzxHeader(await reader.ReadAsync(RzxHeader.HeaderLength).ConfigureAwait(false));
        if (!header.IsValid)
        {
            throw new InvalidDataException("Not a valid RZX file.");
        }
        if (!header.IsSupportedVersion)
        {
            throw new InvalidDataException($"RZX version {header.MajorVersion}.{header.MinorVersion} is not supported.");
        }

        var blocks = new List<RzxBlock>();
        while (!await reader.AtEndAsync().ConfigureAwait(false))
        {
            blocks.Add(await ReadBlockAsync(reader).ConfigureAwait(false));
        }

        ValidateBlocks(blocks);

        return new RzxFile(blocks, header);
    }

    [MustUseReturnValue]
    private static async ValueTask<RzxBlock> ReadBlockAsync(IBinaryReader reader)
    {
        var header = new RzxBlockHeader(await reader.ReadAsync(RzxBlockHeader.Size).ConfigureAwait(false));
        var data = await reader.ReadAsync(checked((int)header.DataLength)).ConfigureAwait(false);

        return header.Type switch
        {
            RzxBlockType.Creator => new RzxCreatorBlock(header, data),
            RzxBlockType.Snapshot => new RzxSnapshotBlock(header, data),
            RzxBlockType.InputRecording => new RzxInputRecordingBlock(header, data),
            RzxBlockType.SecurityInformation or RzxBlockType.SecuritySignature => throw new NotSupportedException($"RZX block type {header.Type} is not supported."),
            _ => throw new NotSupportedException($"RZX block type 0x{(byte)header.Type:X2} is not supported.")
        };
    }

    private static void ValidateBlocks(IReadOnlyList<RzxBlock> blocks)
    {
        if (!blocks.OfType<RzxCreatorBlock>().Any())
        {
            throw new InvalidDataException("RZX files must contain a creator block.");
        }
        if (!blocks.OfType<RzxInputRecordingBlock>().Any())
        {
            throw new InvalidDataException("RZX files must contain an input recording block.");
        }
    }

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(RzxFile file, IBinaryWriter writer)
    {
        await file.Header.WriteAsync(writer).ConfigureAwait(false);

        foreach (var block in file.Blocks)
        {
            await block.Header.WriteAsync(writer).ConfigureAwait(false);
            await block.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}