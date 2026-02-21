namespace MrKWatkins.OakIO.ZXSpectrum.Nex;

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