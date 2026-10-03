using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for tiles file formats.
/// </summary>
public abstract class TilesFormat : ResourceFormat
{
    /// <summary>
    /// Initializes a tiles format.
    /// </summary>
    /// <param name="name">The display name.</param>
    /// <param name="fileExtension">The file extension without a leading dot.</param>
    /// <param name="fileType">The concrete tiles file type.</param>
    protected TilesFormat(string name, string fileExtension, Type fileType)
        : base(name, fileExtension, ValidateFileType(fileType, typeof(TilesFile)))
    {
    }

    /// <summary>
    /// Reads a tiles file from a byte array.
    /// </summary>
    /// <param name="bytes">The file bytes.</param>
    /// <returns>The file read from the bytes.</returns>
    [Pure]
    public new TilesFile Read(byte[] bytes) => (TilesFile)base.Read(bytes);

    /// <summary>
    /// Reads a tiles file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new TilesFile Read(Stream stream) => (TilesFile)base.Read(stream);

    /// <summary>
    /// Reads a tiles file asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new async Task<TilesFile> ReadAsync(Stream stream, CancellationToken cancellationToken = default) =>
        (TilesFile)await base.ReadAsync(stream, cancellationToken).ConfigureAwait(false);

}

/// <summary>
/// Base class for tiles formats with a strongly typed file type.
/// </summary>
/// <typeparam name="TFile">The concrete file type.</typeparam>
public abstract class TilesFormat<TFile>(string name, string fileExtension) : TilesFormat(name, fileExtension, typeof(TFile))
    where TFile : TilesFile
{

    /// <summary>
    /// Reads a tiles file from a byte array.
    /// </summary>
    /// <param name="bytes">The file bytes.</param>
    /// <returns>The file read from the bytes.</returns>
    [Pure]
    public new TFile Read(byte[] bytes) => (TFile)base.Read(bytes);

    /// <summary>
    /// Reads a tiles file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new TFile Read(Stream stream) => (TFile)base.Read(stream);

    /// <summary>
    /// Reads a tiles file asynchronously.
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