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

    public IOFileFormat SourceFormat { get; }

    public IOFileFormat TargetFormat { get; }

    /// <summary>
    /// Converts the given source file to the target format.
    /// </summary>
    [Pure]
    public abstract IOFile Convert(IOFile source);
}

/// <summary>
/// Converts files from one format to another.
/// </summary>
public abstract class IOFileConverter<TSource, TTarget> : IOFileConverter
    where TSource : IOFile
    where TTarget : IOFile
{
    private protected IOFileConverter(IOFileFormat sourceFormat, IOFileFormat targetFormat)
        : base(sourceFormat, targetFormat)
    {
    }

    public sealed override IOFile Convert(IOFile source) =>
        source is TSource typedSource ? Convert(typedSource) : throw new ArgumentException($"Value is not of type {typeof(TSource).Name}.", nameof(source));

    /// <summary>
    /// Converts the given source file to the target format.
    /// </summary>
    [Pure]
    public abstract TTarget Convert(TSource source);
}