namespace MrKWatkins.OakIO;

/// <summary>
/// A trailer with no data.
/// </summary>
public sealed class EmptyTrailer : Trailer
{
    /// <summary>
    /// Gets the singleton instance of <see cref="EmptyTrailer" />.
    /// </summary>
    public static EmptyTrailer Instance { get; } = new();

    private EmptyTrailer()
        : base([])
    {
    }
}