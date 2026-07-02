using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape;

/// <summary>
/// Base class for ZX Spectrum tape file formats.
/// </summary>
/// <param name="name">The name of the format.</param>
/// <param name="fileExtension">The file extension for the format.</param>
/// <param name="fileType">The type of file this format reads and writes.</param>
public abstract class ZXSpectrumTapeFormat(string name, string fileExtension, Type fileType) : ZXSpectrumFileFormat(name, fileExtension, fileType)
{
    /// <summary>
    /// The T-states per second used for tape loading/saving.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public const decimal TStatesPerSecond = 50.08M * 69888;

    /// <summary>
    /// Reads a ZX Spectrum tape file from a byte array.
    /// </summary>
    /// <param name="bytes">The byte array to read from.</param>
    /// <returns>The tape file read from the byte array.</returns>
    [Pure]
    public new ZXSpectrumTapeFile Read(byte[] bytes) => (ZXSpectrumTapeFile)base.Read(bytes);

    /// <summary>
    /// Reads a ZX Spectrum tape file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The tape file read from the stream.</returns>
    [MustUseReturnValue]
    public new ZXSpectrumTapeFile Read(Stream stream) => (ZXSpectrumTapeFile)base.Read(stream);

    /// <summary>
    /// Reads a ZX Spectrum tape file from a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the reading.</param>
    /// <returns>The tape file read from the stream.</returns>
    [MustUseReturnValue]
    public new async Task<ZXSpectrumTapeFile> ReadAsync(Stream stream, CancellationToken cancellationToken = default) =>
        (ZXSpectrumTapeFile)await base.ReadAsync(stream, cancellationToken).ConfigureAwait(false);
}

/// <summary>
/// strongly typed base class for ZX Spectrum tape file formats.
/// </summary>
/// <typeparam name="TFile">The type of tape file this format reads and writes.</typeparam>
/// <param name="name">The name of the format.</param>
/// <param name="fileExtension">The file extension for the format.</param>
public abstract class ZXSpectrumTapeFormat<TFile>(string name, string fileExtension) : ZXSpectrumTapeFormat(name, fileExtension, typeof(TFile))
    where TFile : ZXSpectrumTapeFile
{
    /// <summary>
    /// Reads a tape file from a byte array.
    /// </summary>
    /// <param name="bytes">The byte array to read from.</param>
    /// <returns>The tape file read from the byte array.</returns>
    [Pure]
    public new TFile Read(byte[] bytes) => (TFile)base.Read(bytes);

    /// <summary>
    /// Reads a tape file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The tape file read from the stream.</returns>
    [MustUseReturnValue]
    public new TFile Read(Stream stream) => (TFile)base.Read(stream);

    /// <summary>
    /// Reads a tape file from a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the reading.</param>
    /// <returns>The tape file read from the stream.</returns>
    [MustUseReturnValue]
    public new async Task<TFile> ReadAsync(Stream stream, CancellationToken cancellationToken = default) =>
        (TFile)await base.ReadAsync(stream, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    protected internal sealed override ValueTask WriteAsync(IOFile file, IBinaryWriter writer) =>
        file is TFile typedFile
            ? WriteAsync(typedFile, writer)
            : throw new ArgumentException($"Value is not of type {typeof(TFile).Name}.", nameof(file));

    /// <summary>
    /// Writes a strongly typed tape file to a <see cref="IBinaryWriter" />.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <param name="writer">The <see cref="IBinaryWriter" /> to write to.</param>
    protected abstract ValueTask WriteAsync(TFile file, IBinaryWriter writer);
}