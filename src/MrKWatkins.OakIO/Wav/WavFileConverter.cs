namespace MrKWatkins.OakIO.Wav;

public abstract class WavFileConverter<TSource>(IOFileFormat sourceFormat) : IOFileConverter<TSource, WavFile>(sourceFormat, WavFormat.Instance), IWavFileConverter
    where TSource : IOFile
{
    public sealed override WavFile Convert(TSource source) => Convert(source);

    [Pure]
    public WavFile Convert(IOFile source, uint sampleRateHz = IWavFileConverter.DefaultSampleRateHz) =>
        source is TSource typedSource ? Convert(typedSource, sampleRateHz) : throw new ArgumentException($"Value is not of type {typeof(TSource).Name}.", nameof(source));

    [Pure]
    public abstract WavFile Convert(TSource source, uint sampleRateHz = IWavFileConverter.DefaultSampleRateHz);
}