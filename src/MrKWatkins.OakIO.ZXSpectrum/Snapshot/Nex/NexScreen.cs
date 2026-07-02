namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

/// <summary>
/// A loading screen in a NEX file.
/// </summary>
public sealed class NexScreen
{
    private readonly byte[] data;

    internal NexScreen(NexScreenType type, byte[] data)
    {
        Type = type;
        this.data = data;
    }

    /// <summary>
    /// Gets the screen type.
    /// </summary>
    public NexScreenType Type { get; }

    /// <summary>
    /// Gets the raw screen data.
    /// </summary>
    public IReadOnlyList<byte> Data => data;

    internal ReadOnlyMemory<byte> DataMemory => data;
}