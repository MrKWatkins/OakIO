namespace MrKWatkins.OakIO.Resources.Aky;

// Track positions are independent of the chip's shared registers.
internal sealed class AkyChannelState
{
    internal int Track { get; set; }
    internal int Position { get; set; }
    internal int Remaining { get; set; }
    internal bool Initial { get; set; }
    internal AkySidState? Sid { get; set; }
}