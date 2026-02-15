namespace MrKWatkins.OakIO;

/// <summary>
/// Converts files from one format to another.
/// </summary>
public interface IFormatConverter<in TSource, out TTarget>
    where TSource : IOFile
    where TTarget : IOFile
{
    /// <summary>
    /// Converts the given source file to the target format.
    /// </summary>
    [Pure]
    TTarget Convert(TSource source);
}
