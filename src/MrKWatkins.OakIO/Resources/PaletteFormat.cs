using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for palette file formats.
/// </summary>
public abstract class PaletteFormat : ResourceFormat
{
    /// <summary>
    /// Initializes a palette format.
    /// </summary>
    /// <param name="name">The display name.</param>
    /// <param name="fileExtension">The file extension without a leading dot.</param>
    /// <param name="fileType">The concrete palette file type.</param>
    protected PaletteFormat(string name, string fileExtension, Type fileType)
        : base(name, fileExtension, ValidateFileType(fileType, typeof(PaletteFile)))
    {
    }

    /// <summary>
    /// Reads a palette file from a byte array.
    /// </summary>
    /// <param name="bytes">The file bytes.</param>
    /// <returns>The file read from the bytes.</returns>
    [Pure]
    public new PaletteFile Read(byte[] bytes) => (PaletteFile)base.Read(bytes);

    /// <summary>
    /// Reads a palette file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new PaletteFile Read(Stream stream) => (PaletteFile)base.Read(stream);

    /// <summary>
    /// Reads a palette file asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new async Task<PaletteFile> ReadAsync(Stream stream, CancellationToken cancellationToken = default) =>
        (PaletteFile)await base.ReadAsync(stream, cancellationToken).ConfigureAwait(false);

}

/// <summary>
/// Base class for palette formats with a strongly typed file type.
/// </summary>
/// <typeparam name="TFile">The concrete file type.</typeparam>
public abstract class PaletteFormat<TFile>(string name, string fileExtension) : PaletteFormat(name, fileExtension, typeof(TFile))
    where TFile : PaletteFile
{

    /// <summary>
    /// Reads a palette file from a byte array.
    /// </summary>
    /// <param name="bytes">The file bytes.</param>
    /// <returns>The file read from the bytes.</returns>
    [Pure]
    public new TFile Read(byte[] bytes) => (TFile)base.Read(bytes);

    /// <summary>
    /// Reads a palette file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new TFile Read(Stream stream) => (TFile)base.Read(stream);

    /// <summary>
    /// Reads a palette file asynchronously.
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