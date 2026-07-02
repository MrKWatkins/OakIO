namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// Represents an RZX input recording file.
/// </summary>
public sealed class RzxFile : ZXSpectrumRecordingFile
{
    /// <summary>
    /// Initialises a new instance of the <see cref="RzxFile" /> class.
    /// </summary>
    /// <param name="blocks">The blocks in the file.</param>
    /// <param name="header">The file header, or <c>null</c> to create a default version 0.13 header.</param>
    public RzxFile(IReadOnlyList<RzxBlock> blocks, RzxHeader? header = null)
        : base(RzxFormat.Instance)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        Header = header ?? new RzxHeader();
        Blocks = blocks;
    }

    /// <summary>
    /// Gets the file header.
    /// </summary>
    public RzxHeader Header { get; }

    /// <summary>
    /// Gets the blocks in the file.
    /// </summary>
    public IReadOnlyList<RzxBlock> Blocks { get; }

    /// <summary>
    /// Gets the creator information block.
    /// </summary>
    public RzxCreatorBlock Creator => Blocks.OfType<RzxCreatorBlock>().First();

    /// <summary>
    /// Gets the snapshot block, or <c>null</c> if the file does not contain one.
    /// </summary>
    public RzxSnapshotBlock? Snapshot => Blocks.OfType<RzxSnapshotBlock>().FirstOrDefault();

    /// <summary>
    /// Gets the input recording blocks.
    /// </summary>
    public IReadOnlyList<RzxInputRecordingBlock> InputRecordings => Blocks.OfType<RzxInputRecordingBlock>().ToList();
}