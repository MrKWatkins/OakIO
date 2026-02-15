namespace MrKWatkins.OakIO;

public sealed class EmptyTrailer : Trailer
{
    public static EmptyTrailer Instance { get; } = new();

    private EmptyTrailer()
        : base([])
    {
    }
}