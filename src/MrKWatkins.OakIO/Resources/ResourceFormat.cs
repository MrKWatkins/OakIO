using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for resource file formats.
/// </summary>
public abstract class ResourceFormat : IOFileFormat
{
    /// <summary>
    /// Initializes a resource format.
    /// </summary>
    /// <param name="name">The display name.</param>
    /// <param name="fileExtension">The file extension without a leading dot.</param>
    /// <param name="fileType">The concrete resource file type.</param>
    protected ResourceFormat(string name, string fileExtension, Type fileType)
        : base(name, fileExtension, ValidateFileType(fileType, typeof(ResourceFile)))
    {
    }

    /// <summary>
    /// Reads a resource file from a byte array.
    /// </summary>
    /// <param name="bytes">The file bytes.</param>
    /// <returns>The file read from the bytes.</returns>
    [Pure]
    public new ResourceFile Read(byte[] bytes) => (ResourceFile)base.Read(bytes);

    /// <summary>
    /// Reads a resource file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new ResourceFile Read(Stream stream) => (ResourceFile)base.Read(stream);

    /// <summary>
    /// Reads a resource file asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new async Task<ResourceFile> ReadAsync(Stream stream, CancellationToken cancellationToken = default) =>
        (ResourceFile)await base.ReadAsync(stream, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Validates that a format's file type derives from the expected resource file type.
    /// </summary>
    /// <param name="fileType">The file type to validate.</param>
    /// <param name="expectedType">The required base file type.</param>
    /// <returns>The validated file type.</returns>
    [Pure]
    protected static Type ValidateFileType(Type fileType, Type expectedType)
    {
        ArgumentNullException.ThrowIfNull(fileType);
        if (fileType == expectedType || !fileType.IsAssignableTo(expectedType))
        {
            throw new ArgumentException($"The specified file type must be a subclass of {expectedType.Name}.", nameof(fileType));
        }
        return fileType;
    }
}

/// <summary>
/// Base class for resource formats with a strongly typed file type.
/// </summary>
/// <typeparam name="TFile">The concrete file type.</typeparam>
public abstract class ResourceFormat<TFile>(string name, string fileExtension) : ResourceFormat(name, fileExtension, typeof(TFile))
    where TFile : ResourceFile
{

    /// <summary>
    /// Reads a resource file from a byte array.
    /// </summary>
    /// <param name="bytes">The file bytes.</param>
    /// <returns>The file read from the bytes.</returns>
    [Pure]
    public new TFile Read(byte[] bytes) => (TFile)base.Read(bytes);

    /// <summary>
    /// Reads a resource file from a stream.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public new TFile Read(Stream stream) => (TFile)base.Read(stream);

    /// <summary>
    /// Reads a resource file asynchronously.
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