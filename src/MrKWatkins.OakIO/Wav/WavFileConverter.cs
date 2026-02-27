namespace MrKWatkins.OakIO.Wav;

/// <summary>
/// Base class for converters that convert a source file to WAV format.
/// </summary>
/// <typeparam name="TSource">The type of the source file to convert.</typeparam>
/// <param name="sourceFormat">The file format of the source file.</param>
public abstract class WavFileConverter<TSource>(IOFileFormat sourceFormat) : IOFileConverter<TSource, WavFile>(sourceFormat, WavFormat.Instance), IWavFileConverter
    where TSource : IOFile
{
    /// <inheritdoc />
    public sealed override WavFile Convert(TSource source) => Convert(source);

    /// <summary>
    /// Converts the specified source file to a WAV file with the given sample rate.
    /// </summary>
    /// <param name="source">The source file to convert.</param>
    /// <param name="sampleRateHz">The sample rate in Hz for the output WAV file.</param>
    /// <returns>The converted WAV file.</returns>
    [Pure]
    public WavFile Convert(IOFile source, uint sampleRateHz = IWavFileConverter.DefaultSampleRateHz) =>
        source is TSource typedSource ? Convert(typedSource, sampleRateHz) : throw new ArgumentException($"Value is not of type {typeof(TSource).Name}.", nameof(source));

    /// <summary>
    /// Converts the specified strongly-typed source file to a WAV file with the given sample rate.
    /// </summary>
    /// <param name="source">The source file to convert.</param>
    /// <param name="sampleRateHz">The sample rate in Hz for the output WAV file.</param>
    /// <returns>The converted WAV file.</returns>
    [Pure]
    public abstract WavFile Convert(TSource source, uint sampleRateHz = IWavFileConverter.DefaultSampleRateHz);
}