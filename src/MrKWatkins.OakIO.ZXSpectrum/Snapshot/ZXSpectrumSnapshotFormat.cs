namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot;

/// <summary>
/// Base class for ZX Spectrum snapshot file formats.
/// </summary>
/// <param name="name">The name of the format.</param>
/// <param name="fileExtension">The file extension for the format.</param>
/// <param name="fileType">The type of file for the format.</param>
public abstract class ZXSpectrumSnapshotFormat(string name, string fileExtension, Type fileType) : IOFileFormat(name, fileExtension, fileType)
{
    /// <summary>
    /// Reads a snapshot file from a byte array.
    /// </summary>
    /// <param name="bytes">The byte array to read from.</param>
    /// <returns>The snapshot file.</returns>
    [Pure]
    public new ZXSpectrumSnapshotFile Read(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return Read(stream);
    }

    /// <inheritdoc />
    public sealed override ZXSpectrumSnapshotFile Read(Stream stream) => ReadSnapshot(stream);

    /// <summary>
    /// Reads a snapshot file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The snapshot file.</returns>
    [MustUseReturnValue]
    protected abstract ZXSpectrumSnapshotFile ReadSnapshot(Stream stream);
}

/// <summary>
/// Base class for ZX Spectrum snapshot file formats with a specific file type.
/// </summary>
/// <typeparam name="TFile">The type of snapshot file.</typeparam>
/// <param name="name">The name of the format.</param>
/// <param name="fileExtension">The file extension for the format.</param>
public abstract class ZXSpectrumSnapshotFormat<TFile>(string name, string fileExtension) : ZXSpectrumSnapshotFormat(name, fileExtension, typeof(TFile))
    where TFile : ZXSpectrumSnapshotFile
{
    /// <summary>
    /// Reads a snapshot file from a byte array.
    /// </summary>
    /// <param name="bytes">The byte array to read from.</param>
    /// <returns>The typed snapshot file.</returns>
    [Pure]
    public new TFile Read(byte[] bytes) => (TFile)base.Read(bytes);

    /// <summary>
    /// Reads a snapshot file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The typed snapshot file.</returns>
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
    /// Writes a snapshot file to a stream.
    /// </summary>
    /// <param name="file">The snapshot file to write.</param>
    /// <param name="stream">The stream to write to.</param>
    protected abstract void Write(TFile file, Stream stream);
}