namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

public sealed class NexScreen
{
    internal NexScreen(NexScreenType type, byte[] data)
    {
        Type = type;
        Data = data;
    }

    public NexScreenType Type { get; }

    public byte[] Data { get; }
}