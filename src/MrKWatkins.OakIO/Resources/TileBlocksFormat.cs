using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for tile blocks file formats.
/// </summary>
public abstract class TileBlocksFormat : ResourceFormat
{
    /// <summary>
    /// Initializes a tile blocks format.
    /// </summary>
    /// <param name="name">The display name.</param>
    /// <param name="fileExtension">The file extension without a leading dot.</param>
    /// <param name="fileType">The concrete tile blocks file type.</param>
    protected TileBlocksFormat(string name, string fileExtension, Type fileType)
        : base(name, fileExtension, ValidateFileType(fileType, typeof(TileBlocksFile)))
    {
    }

    /// <summary>
    /// Reads a tile blocks file from a byte array.
    /// </summary>
    /// <param name="bytes">The file bytes.</param>
    /// <returns>The file read from the bytes.</returns>
    [Pure]
    public new TileBlocksFile Read(byte[] bytes) => (TileBlocksFile)base.Read(bytes);

    /// <summary>
    /// Reads a tile blocks file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new TileBlocksFile Read(Stream stream) => (TileBlocksFile)base.Read(stream);

    /// <summary>
    /// Reads a tile blocks file asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new async Task<TileBlocksFile> ReadAsync(Stream stream, CancellationToken cancellationToken = default) =>
        (TileBlocksFile)await base.ReadAsync(stream, cancellationToken).ConfigureAwait(false);

}

/// <summary>
/// Base class for tile blocks formats with a strongly typed file type.
/// </summary>
/// <typeparam name="TFile">The concrete file type.</typeparam>
public abstract class TileBlocksFormat<TFile>(string name, string fileExtension) : TileBlocksFormat(name, fileExtension, typeof(TFile))
    where TFile : TileBlocksFile
{

    /// <summary>
    /// Reads a tile blocks file from a byte array.
    /// </summary>
    /// <param name="bytes">The file bytes.</param>
    /// <returns>The file read from the bytes.</returns>
    [Pure]
    public new TFile Read(byte[] bytes) => (TFile)base.Read(bytes);

    /// <summary>
    /// Reads a tile blocks file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new TFile Read(Stream stream) => (TFile)base.Read(stream);

    /// <summary>
    /// Reads a tile blocks file asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new async Task<TFile> ReadAsync(Stream stream, CancellationToken cancellationToken = default) =>
        (TFile)await base.ReadAsync(stream, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    protected internal sealed override ValueTask WriteAsync(IOFile file, IBinaryWriter writer) =>
        file is TFile typedFile
            ? WriteAsync(typedFile, writer)
            : throw new ArgumentException($"Value is not of type {typeof(TFile).Name}.", nameof(file));

    /// <summary>
    /// Writes a strongly typed resource file.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <param name="writer">The binary writer.</param>
    protected abstract ValueTask WriteAsync(TFile file, IBinaryWriter writer);
}