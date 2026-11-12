using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for music file formats.
/// </summary>
/// <param name="name">The display name.</param>
/// <param name="fileExtension">The file extension without a leading dot.</param>
/// <param name="fileType">The concrete music file type.</param>
public abstract class MusicFormat(string name, string fileExtension, Type fileType) : ResourceFormat(name, fileExtension, ValidateFileType(fileType, typeof(MusicFile)))
{

    /// <summary>
    /// Reads a music file from a byte array.
    /// </summary>
    /// <param name="bytes">The file bytes.</param>
    /// <returns>The file read from the bytes.</returns>
    [Pure]
    public new MusicFile Read(byte[] bytes) => (MusicFile)base.Read(bytes);

    /// <summary>
    /// Reads a music file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new MusicFile Read(Stream stream) => (MusicFile)base.Read(stream);

    /// <summary>
    /// Reads a music file asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new async Task<MusicFile> ReadAsync(Stream stream, CancellationToken cancellationToken = default) =>
        (MusicFile)await base.ReadAsync(stream, cancellationToken).ConfigureAwait(false);

}

/// <summary>
/// Base class for music formats with a strongly typed file type.
/// </summary>
/// <typeparam name="TFile">The concrete file type.</typeparam>
public abstract class MusicFormat<TFile>(string name, string fileExtension) : MusicFormat(name, fileExtension, typeof(TFile))
    where TFile : MusicFile
{

    /// <summary>
    /// Reads a music file from a byte array.
    /// </summary>
    /// <param name="bytes">The file bytes.</param>
    /// <returns>The file read from the bytes.</returns>
    [Pure]
    public new TFile Read(byte[] bytes) => (TFile)base.Read(bytes);

    /// <summary>
    /// Reads a music file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new TFile Read(Stream stream) => (TFile)base.Read(stream);

    /// <summary>
    /// Reads a music file asynchronously.
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