using System.IO.Compression;

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
    /// Writes a file to a directory with the specified name.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <param name="directory">The directory to write the file to.</param>
    /// <param name="name">The name of the file without extension.</param>
    /// <param name="zipped">Whether to write the file inside a ZIP archive.</param>
    public void Write(IOFile file, [PathReference] string directory, string name, bool zipped = false) => Write(file, Path.Combine(directory, GetFilename(name)), zipped);

    /// <summary>
    /// Writes a file to disk.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <param name="filePath">The path to write the file to.</param>
    /// <param name="zipped">Whether to write the file inside a ZIP archive.</param>
    public void Write(IOFile file, [PathReference] string filePath, bool zipped = false)
    {
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Extension != $".{FileExtension}")
        {
            throw new ArgumentException($"Value has the extension {fileInfo.Extension} rather than the expected .{FileExtension}.", nameof(filePath));
        }

        var filename = fileInfo.Name;
        using var stream = File.Create(Path.Combine(fileInfo.DirectoryName!, zipped ? $"{filename}.zip" : filename));
        if (zipped)
        {
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create);
            var entry = zip.CreateEntry(filename);
            using var entryStream = entry.Open();
            Write(file, entryStream);
        }
        else
        {
            Write(file, stream);
        }
    }

    /// <summary>
    /// Writes a file to a byte array.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <returns>A byte array containing the file data.</returns>
    [Pure]
    public byte[] Write(IOFile file)
    {
        using var memoryStream = new MemoryStream();
        Write(file, memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Writes a file to a stream.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <param name="stream">The stream to write to.</param>
    public abstract void Write(IOFile file, Stream stream);
}

/// <summary>
/// Base class for file formats with a strongly-typed file type.
/// </summary>
/// <typeparam name="TFile">The type of file for this format.</typeparam>
public abstract class IOFileFormat<TFile>(string name, string fileExtension) : IOFileFormat(name, fileExtension, typeof(TFile))
    where TFile : IOFile
{
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
    /// Writes a strongly-typed file to a stream.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <param name="stream">The stream to write to.</param>
    protected abstract void Write(TFile file, Stream stream);
}