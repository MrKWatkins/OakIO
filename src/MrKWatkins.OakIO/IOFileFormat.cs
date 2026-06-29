using MrKWatkins.OakIO.Compression;

namespace MrKWatkins.OakIO;

/// <summary>
/// Base class for file formats, providing information about the format and methods for reading and writing files.
/// </summary>
public abstract class IOFileFormat
{
    private int convertersRegistered;

    /// <summary>
    /// Initialises a new instance of the <see cref="IOFileFormat" /> class.
    /// </summary>
    /// <param name="name">The display name of the format.</param>
    /// <param name="fileExtension">The file extension for the format, without a leading dot.</param>
    /// <param name="fileType">The type of <see cref="IOFile" /> for this format.</param>
    protected IOFileFormat(string name, string fileExtension, Type fileType)
    {
        if (!fileType.IsAssignableTo(typeof(IOFile)))
        {
            throw new ArgumentException($"Value is not of type {nameof(IOFile)}.", nameof(fileType));
        }

        Name = name;
        FileExtension = fileExtension;
        FileType = fileType;
    }

    /// <summary>
    /// Gets the display name of this format.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the file extension for this format, without a leading dot.
    /// </summary>
    public string FileExtension { get; }

    /// <summary>
    /// Gets the type of <see cref="IOFile" /> for this format.
    /// </summary>
    public Type FileType { get; }

    /// <summary>
    /// Gets a value indicating whether this format supports reading.
    /// </summary>
    public virtual bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether this format supports writing.
    /// </summary>
    public virtual bool CanWrite => true;

    /// <summary>
    /// Call from the IOFile to ensure that converters are registered for this format. The converters depend on the IOFileFormat static Instance field,
    /// which will not be assigned until the IOFileFormat is fully constructed. Therefore, we cannot register the converters in the constructor and do
    /// it in the IOFile constructor instead.
    /// </summary>
    internal void EnsureConvertersAreRegistered()
    {
        if (Interlocked.CompareExchange(ref convertersRegistered, 1, 0) == 0)
        {
            IOFileConversion.RegisterConverters(CreateConverters());
        }
    }

    /// <summary>
    /// Creates the converters for this format.
    /// </summary>
    /// <returns>The converters for this format.</returns>
    [Pure]
    protected virtual IEnumerable<IOFileConverter> CreateConverters() => [];

    /// <summary>
    /// Gets the filename for a file of this format with the specified name.
    /// </summary>
    /// <param name="name">The name of the file without extension.</param>
    /// <returns>The filename with extension.</returns>
    [Pure]
    public string GetFilename(string name) => $"{name}.{FileExtension}";

    /// <summary>
    /// Loads a file from disk.
    /// </summary>
    /// <param name="path">The path to the file to load. </param>
    /// <param name="supportedFormats">The supported formats for the file.</param>
    /// <returns>The file that was read.</returns>
    [Pure]
    public static IOFile Load([PathReference] string path, params IReadOnlyList<IOFileFormat> supportedFormats)
    {
        using var fileStream = File.OpenRead(path);
        return Codec.Load(path, fileStream, supportedFormats);
    }

    /// <summary>
    /// Loads a file from a stream. Use as an alternative to <see cref="Load(string, IReadOnlyList{IOFileFormat})"/> when the file is not on disk, e.g. web upload.
    /// </summary>
    /// <param name="path">The path of the file in the stream. Used to determine the type of the file.</param>
    /// <param name="stream">The file.</param>
    /// <param name="supportedFormats">The supported formats for the file.</param>
    /// <returns>The file that was read.</returns>
    [MustUseReturnValue]
    public static IOFile Load([PathReference] string path, Stream stream, params IReadOnlyList<IOFileFormat> supportedFormats) => Codec.Load(path, stream, supportedFormats);

    /// <summary>
    /// Loads a file from disk asynchronously.
    /// </summary>
    /// <param name="path">The path to the file to load. </param>
    /// <param name="supportedFormats">The supported formats for the file.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the loading.</param>
    /// <returns>The file that was read.</returns>
    [Pure]
    public static async Task<IOFile> LoadAsync([PathReference] string path, IReadOnlyList<IOFileFormat> supportedFormats, CancellationToken cancellationToken = default)
    {
        var fileStream = File.OpenRead(path);
        await using (fileStream.ConfigureAwait(false))
        {
            return await Codec.LoadAsync(path, fileStream, supportedFormats, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Loads a file from a stream asynchronously. Use as an alternative to <see cref="LoadAsync(string, IReadOnlyList{IOFileFormat}, CancellationToken)"/> when the file is not on disk, e.g. web upload.
    /// </summary>
    /// <param name="path">The path of the file in the stream. Used to determine the type of the file.</param>
    /// <param name="stream">The file.</param>
    /// <param name="supportedFormats">The supported formats for the file.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the loading.</param>
    /// <returns>The file that was read.</returns>
    [MustUseReturnValue]
    public static Task<IOFile> LoadAsync([PathReference] string path, Stream stream, IReadOnlyList<IOFileFormat> supportedFormats, CancellationToken cancellationToken = default) =>
        Codec.LoadAsync(path, stream, supportedFormats, cancellationToken);

    /// <summary>
    /// Reads a file from a byte array.
    /// </summary>
    /// <param name="bytes">The byte array to read from.</param>
    /// <returns>The file that was read.</returns>
    [Pure]
    public IOFile Read(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return Read(stream);
    }

    /// <summary>
    /// Reads a file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The file that was read.</returns>
    [MustUseReturnValue]
    public abstract IOFile Read(Stream stream);

    /// <summary>
    /// Reads a file from a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the reading.</param>
    /// <returns>The file that was read.</returns>
    [MustUseReturnValue]
    public virtual Task<IOFile> ReadAsync(Stream stream, CancellationToken cancellationToken = default) => Task.FromResult(Read(stream));

    /// <summary>
    /// Writes a file to a stream.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <param name="stream">The stream to write to.</param>
    protected internal abstract void Write(IOFile file, Stream stream);

    /// <summary>
    /// Writes a file to a stream asynchronously.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the writing.</param>
    protected internal virtual Task WriteAsync(IOFile file, Stream stream, CancellationToken cancellationToken = default)
    {
        Write(file, stream);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Base class for file formats with a strongly typed file type.
/// </summary>
/// <typeparam name="TFile">The type of file for this format.</typeparam>
public abstract class IOFileFormat<TFile>(string name, string fileExtension) : IOFileFormat(name, fileExtension, typeof(TFile))
    where TFile : IOFile
{
    /// <inheritdoc />
    protected internal sealed override void Write(IOFile file, Stream stream)
    {
        if (file is not TFile typedFile)
        {
            throw new ArgumentException($"Value is not of type {typeof(TFile).Name}.", nameof(file));
        }

        Write(typedFile, stream);
    }

    /// <summary>
    /// Writes a strongly typed file to a stream.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <param name="stream">The stream to write to.</param>
    protected abstract void Write(TFile file, Stream stream);

    /// <inheritdoc />
    protected internal sealed override Task WriteAsync(IOFile file, Stream stream, CancellationToken cancellationToken = default) =>
        file is TFile typedFile
            ? WriteAsync(typedFile, stream, cancellationToken)
            : throw new ArgumentException($"Value is not of type {typeof(TFile).Name}.", nameof(file));

    /// <summary>
    /// Writes a strongly typed file to a stream.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="cancellationToken">An <see cref="CancellationToken"/> to cancel the writing.</param>
    protected virtual Task WriteAsync(TFile file, Stream stream, CancellationToken cancellationToken)
    {
        Write(file, stream);
        return Task.CompletedTask;
    }
}