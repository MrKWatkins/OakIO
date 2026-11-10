namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>A linked view of the native block references used by a pattern channel.</summary>
public sealed class AkyTrack
{
    private readonly AkyFile file;
    private readonly int duration;

    internal AkyTrack(AkyFile file, int offset, int duration)
    {
        this.file = file;
        Offset = offset;
        this.duration = duration;
    }

    /// <summary>Gets the track's file offset.</summary>
    public int Offset { get; }

    /// <summary>Gets the block references covering this pattern's duration.</summary>
    public IReadOnlyList<AkyTrackBlock> Blocks
    {
        get
        {
            var blocks = new List<AkyTrackBlock>();
            var remaining = duration;
            var offset = Offset;
            while (remaining > 0)
            {
                file.CheckRange(offset, 3);
                var block = new AkyTrackBlock(file, offset);
                file.CheckRange(block.RegisterBlockOffset, 1);
                blocks.Add(block);
                remaining -= block.Duration;
                offset += 3;
            }
            return blocks;
        }
    }
}