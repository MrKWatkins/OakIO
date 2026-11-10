namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>The native linker, tracks and compressed register blocks after the header.</summary>
public sealed class AkyData : IOFileComponent
{
    internal AkyData(byte[] bytes) : base(bytes)
    {
    }
}