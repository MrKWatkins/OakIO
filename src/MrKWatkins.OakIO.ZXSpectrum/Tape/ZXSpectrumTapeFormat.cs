namespace MrKWatkins.OakIO.ZXSpectrum.Tape;

/// <summary>
/// Base class for ZX Spectrum tape file formats.
/// </summary>
/// <param name="name">The name of the format.</param>
/// <param name="fileExtension">The file extension for the format.</param>
/// <param name="fileType">The type of file this format reads and writes.</param>
public abstract class ZXSpectrumTapeFormat(string name, string fileExtension, Type fileType) : IOFileFormat(name, fileExtension, fileType)
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
    public new ZXSpectrumTapeFile Read(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return Read(stream);
    }

    /// <inheritdoc />
    public sealed override ZXSpectrumTapeFile Read(Stream stream) => ReadTape(stream);

    /// <summary>
    /// Reads a ZX Spectrum tape file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The tape file read from the stream.</returns>
    [MustUseReturnValue]
    protected abstract ZXSpectrumTapeFile ReadTape(Stream stream);
}

/// <summary>
/// Strongly-typed base class for ZX Spectrum tape file formats.
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

    /// <inheritdoc />
    public sealed override void Write(IOFile file, Stream stream)
    {
        if (file is not TFile typedFile)
        {
            throw new ArgumentException($"Value is not of type {typeof(TFile).Name}.", nameof(file));
        }

        Write(typedFile, stream);
    }

    /// <summary>
    /// Writes a tape file to a stream.
    /// </summary>
    /// <param name="file">The tape file to write.</param>
    /// <param name="stream">The stream to write to.</param>
    protected abstract void Write(TFile file, Stream stream);
}