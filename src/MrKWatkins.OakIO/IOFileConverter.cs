namespace MrKWatkins.OakIO;

/// <summary>
/// Converts files from one format to another.
/// </summary>
public abstract class IOFileConverter
{
    private protected IOFileConverter(IOFileFormat sourceFormat, IOFileFormat targetFormat)
    {
        SourceFormat = sourceFormat;
        TargetFormat = targetFormat;
    }

    /// <summary>
    /// Gets the source format of the converter.
    /// </summary>
    public IOFileFormat SourceFormat { get; }

    /// <summary>
    /// Gets the target format of the converter.
    /// </summary>
    public IOFileFormat TargetFormat { get; }

    /// <summary>
    /// Converts the given source file to the target format.
    /// </summary>
    /// <param name="source">The source file to convert.</param>
    /// <returns>The converted file.</returns>
    [Pure]
    public abstract IOFile Convert(IOFile source);
}

/// <summary>
/// Converts files from one format to another.
/// </summary>
/// <typeparam name="TSource">The source file type.</typeparam>
/// <typeparam name="TTarget">The target file type.</typeparam>
public abstract class IOFileConverter<TSource, TTarget> : IOFileConverter
    where TSource : IOFile
    where TTarget : IOFile
{
    private protected IOFileConverter(IOFileFormat sourceFormat, IOFileFormat targetFormat)
        : base(sourceFormat, targetFormat)
    {
    }

    /// <inheritdoc />
    public sealed override IOFile Convert(IOFile source) =>
        source is TSource typedSource ? Convert(typedSource) : throw new ArgumentException($"Value is not of type {typeof(TSource).Name}.", nameof(source));

    /// <summary>
    /// Converts the given source file to the target format.
    /// </summary>
    /// <param name="source">The source file to convert.</param>
    /// <returns>The converted file.</returns>
    [Pure]
    public abstract TTarget Convert(TSource source);
}