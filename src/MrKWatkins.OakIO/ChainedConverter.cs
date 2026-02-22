namespace MrKWatkins.OakIO;

internal sealed class ChainedConverter<TSource, TIntermediate, TTarget>(IOFileConverter<TSource, TIntermediate> sourceToIntermediateConverter, IOFileConverter<TIntermediate, TTarget> intermediateToTargetConverter)
    : IOFileConverter<TSource, TTarget>(sourceToIntermediateConverter.SourceFormat, intermediateToTargetConverter.TargetFormat)
    where TSource : IOFile
    where TIntermediate : IOFile
    where TTarget : IOFile
{
    public override TTarget Convert(TSource source) => intermediateToTargetConverter.Convert(sourceToIntermediateConverter.Convert(source));
}