using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO;

/// <summary>
/// Provides static methods to convert between file formats using registered converters.
/// </summary>
public static class IOFileConversion
{
    private static readonly Lock Lock = new();
    private static readonly Dictionary<(IOFileFormat Source, IOFileFormat Target), IOFileConverter> Converters = new();
    private static readonly Dictionary<(IOFileFormat Source, Type Target), IOFileConverter> ConvertersByTargetFileType = new();

    internal static void RegisterConverters([InstantHandle] params IEnumerable<IOFileConverter> converters)
    {
        foreach (var converter in converters)
        {
            var key = (converter.SourceFormat, converter.TargetFormat);
            lock (Lock)
            {
                if (Converters.TryGetValue(key, out var existingConverter))
                {
                    if (existingConverter.GetType() != converter.GetType())
                    {
                        throw new InvalidOperationException($"Converter for {key.SourceFormat.Name} to {key.TargetFormat.Name} already registered with type {existingConverter.GetType().Name}.");
                    }

                    continue;
                }

                Converters[key] = converter;
                ConvertersByTargetFileType[(converter.SourceFormat, converter.TargetFormat.FileType)] = converter;
            }
        }
    }

    /// <summary>
    /// Converts a file to the specified target file type.
    /// </summary>
    /// <typeparam name="TTarget">The target file type.</typeparam>
    /// <param name="source">The source file to convert.</param>
    /// <returns>The converted file.</returns>
    [Pure]
    public static TTarget Convert<TTarget>(IOFile source)
        where TTarget : IOFile
    {
        IOFileConverter converter;
        lock (Lock)
        {
            converter = ConvertersByTargetFileType.GetValueOrDefault((source.Format, typeof(TTarget)))
                        ?? throw new InvalidOperationException($"No converter registered for {source.Format.Name} to {typeof(TTarget).Name}.");
        }

        return (TTarget)converter.Convert(source);
    }

    /// <summary>
    /// Converts a file to the specified target format.
    /// </summary>
    /// <param name="source">The source file to convert.</param>
    /// <param name="targetFormat">The target format to convert to.</param>
    /// <returns>The converted file.</returns>
    [Pure]
    public static IOFile Convert(IOFile source, IOFileFormat targetFormat)
    {
        IOFileConverter converter;
        lock (Lock)
        {
            converter = Converters.GetValueOrDefault((source.Format, targetFormat))
                        ?? throw new InvalidOperationException($"No converter registered for {source.Format.Name} to {targetFormat.Name}.");
        }

        return converter.Convert(source);
    }

    /// <summary>
    /// Converts a file to the specified target type.
    /// </summary>
    /// <param name="source">The source file to convert.</param>
    /// <param name="targetType">The target <see cref="IOFile" /> type to convert to.</param>
    /// <returns>The converted file.</returns>
    [Pure]
    public static IOFile Convert(IOFile source, Type targetType)
    {
        if (!targetType.IsAssignableTo(typeof(IOFile)))
        {
            throw new ArgumentException("Value must be an IOFile.", nameof(targetType));
        }

        IOFileConverter converter;
        lock (Lock)
        {
            converter = ConvertersByTargetFileType.GetValueOrDefault((source.Format, targetType))
                        ?? throw new InvalidOperationException($"No converter registered for {source.Format.Name} to {targetType.Name}.");
        }

        return converter.Convert(source);
    }

    /// <summary>
    /// Converts a file to WAV format with an optional custom sample rate.
    /// </summary>
    /// <param name="source">The source file to convert.</param>
    /// <param name="sampleRateHz">The sample rate in Hz.</param>
    /// <returns>The converted WAV file.</returns>
    [Pure]
    public static WavFile ConvertToWav(IOFile source, uint sampleRateHz = IWavFileConverter.DefaultSampleRateHz)
    {
        IOFileConverter converter;
        lock (Lock)
        {
            converter = ConvertersByTargetFileType.GetValueOrDefault((source.Format, typeof(WavFile)))
                        ?? throw new InvalidOperationException($"No converter registered for {source.Format.Name} to {nameof(WavFile)}.");
        }

        return converter is IWavFileConverter wavConverter
            ? wavConverter.Convert(source, sampleRateHz)
            : throw new InvalidOperationException($"Converter for {source.Format.Name} to {nameof(WavFile)} does not support custom sample rates.");
    }

    /// <summary>
    /// Attempts to convert a file to the specified target format, returning <c>false</c> if the conversion fails.
    /// </summary>
    /// <param name="source">The source file to convert.</param>
    /// <param name="targetFormat">The target format to convert to.</param>
    /// <param name="result">The converted file, or <c>null</c> if conversion failed.</param>
    /// <param name="error">The error message if conversion failed, or <c>null</c> if successful.</param>
    /// <returns><c>true</c> if the conversion succeeded; <c>false</c> otherwise.</returns>
    public static bool TryConvert(IOFile source, IOFileFormat targetFormat, [NotNullWhen(true)] out IOFile? result, [NotNullWhen(false)] out string? error)
    {
        try
        {
            result = Convert(source, targetFormat);
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            result = null;
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Gets the formats that the specified source format can be converted to.
    /// </summary>
    /// <param name="sourceFormat">The source format.</param>
    /// <returns>The list of supported target formats.</returns>
    [Pure]
    public static IReadOnlyList<IOFileFormat> GetSupportedConversionFormats(IOFileFormat sourceFormat)
    {
        lock (Lock)
        {
            return Converters.Keys
                .Where(key => key.Source == sourceFormat)
                .Select(key => key.Target)
                .OrderBy(converter => converter.Name)
                .ToList();
        }
    }
}