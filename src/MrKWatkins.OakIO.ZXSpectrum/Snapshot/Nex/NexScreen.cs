namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

/// <summary>
/// A loading screen in a NEX file.
/// </summary>
public sealed class NexScreen
{
    internal NexScreen(NexScreenType type, byte[] data)
    {
        Type = type;
        Data = data;
    }

    /// <summary>
    /// Gets the screen type.
    /// </summary>
    public NexScreenType Type { get; }

    /// <summary>
    /// Gets the raw screen data.
    /// </summary>
    public byte[] Data { get; }
}