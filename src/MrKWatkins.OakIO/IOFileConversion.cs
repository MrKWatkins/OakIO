using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO;

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