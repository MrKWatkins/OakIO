using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.ZXSpectrum.Recording;

/// <summary>
/// Base class for ZX Spectrum input recording file formats.
/// </summary>
/// <param name="name">The name of the format.</param>
/// <param name="fileExtension">The file extension for the format.</param>
/// <param name="fileType">The type of file this format reads and writes.</param>
public abstract class ZXSpectrumRecordingFormat(string name, string fileExtension, Type fileType) : ZXSpectrumFileFormat(name, fileExtension, fileType)
{
    /// <summary>
    /// Reads a recording file from a byte array.
    /// </summary>
    /// <param name="bytes">The byte array to read from.</param>
    /// <returns>The recording file read from the byte array.</returns>
    [Pure]
    public new ZXSpectrumRecordingFile Read(byte[] bytes) => (ZXSpectrumRecordingFile)base.Read(bytes);

    /// <summary>
    /// Reads a recording file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The recording file read from the stream.</returns>
    [MustUseReturnValue]
    public new ZXSpectrumRecordingFile Read(Stream stream) => (ZXSpectrumRecordingFile)base.Read(stream);

    /// <summary>
    /// Reads a recording file from a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the reading.</param>
    /// <returns>The recording file read from the stream.</returns>
    [MustUseReturnValue]
    public new async Task<ZXSpectrumRecordingFile> ReadAsync(Stream stream, CancellationToken cancellationToken = default) =>
        (ZXSpectrumRecordingFile)await base.ReadAsync(stream, cancellationToken).ConfigureAwait(false);
}

/// <summary>
/// Strongly typed base class for ZX Spectrum input recording file formats.
/// </summary>
/// <typeparam name="TFile">The type of recording file this format reads and writes.</typeparam>
/// <param name="name">The name of the format.</param>
/// <param name="fileExtension">The file extension for the format.</param>
public abstract class ZXSpectrumRecordingFormat<TFile>(string name, string fileExtension) : ZXSpectrumRecordingFormat(name, fileExtension, typeof(TFile))
    where TFile : ZXSpectrumRecordingFile
{
    /// <summary>
    /// Reads a recording file from a byte array.
    /// </summary>
    /// <param name="bytes">The byte array to read from.</param>
    /// <returns>The recording file read from the byte array.</returns>
    [Pure]
    public new TFile Read(byte[] bytes) => (TFile)base.Read(bytes);

    /// <summary>
    /// Reads a recording file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The recording file read from the stream.</returns>
    [MustUseReturnValue]
    public new TFile Read(Stream stream) => (TFile)base.Read(stream);

    /// <summary>
    /// Reads a recording file from a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the reading.</param>
    /// <returns>The recording file read from the stream.</returns>
    [MustUseReturnValue]
    public new async Task<TFile> ReadAsync(Stream stream, CancellationToken cancellationToken = default) =>
        (TFile)await base.ReadAsync(stream, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    protected internal sealed override ValueTask WriteAsync(IOFile file, IBinaryWriter writer) =>
        file is TFile typedFile
            ? WriteAsync(typedFile, writer)
            : throw new ArgumentException($"Value is not of type {typeof(TFile).Name}.", nameof(file));

    /// <summary>
    /// Writes a strongly typed recording file to a <see cref="IBinaryWriter" />.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <param name="writer">The <see cref="IBinaryWriter" /> to write to.</param>
    protected abstract ValueTask WriteAsync(TFile file, IBinaryWriter writer);
}