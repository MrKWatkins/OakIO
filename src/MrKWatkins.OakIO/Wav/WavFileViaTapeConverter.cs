using MrKWatkins.OakIO.Tape;

namespace MrKWatkins.OakIO.Wav;

internal sealed class WavFileViaTapeConverter<TSource>(IOFileFormat sourceFormat, IOFileConverter<TSource, TapeFile> tapeConverter, TapeToWavConverter tapeToWavConverter) : WavFileConverter<TSource>(sourceFormat)
    where TSource : IOFile
{
    public override WavFile Convert(TSource source, uint sampleRateHz = IWavFileConverter.DefaultSampleRateHz)
    {
        var tape = tapeConverter.Convert(source);

        return tapeToWavConverter.Convert(tape, sampleRateHz);
    }
}