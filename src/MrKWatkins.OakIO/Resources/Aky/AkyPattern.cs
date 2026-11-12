namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>A linked view of one native AKY pattern entry.</summary>
public sealed class AkyPattern
{
    private readonly AkyFile file;

    internal AkyPattern(AkyFile file, int offset)
    {
        this.file = file;
        Offset = offset;
    }

    /// <summary>Gets this entry's byte offset from the beginning of the file.</summary>
    public int Offset { get; }

    /// <summary>Gets the pattern duration in playback frames.</summary>
    public int Duration => file.ReadWord(Offset);

    /// <summary>Gets linked track views in encoded channel order.</summary>
    public IReadOnlyList<AkyTrack> Tracks => Enumerable.Range(0, file.Header.ChannelCount)
        .Select(channel => new AkyTrack(file, file.ReadPointer(Offset + 2 + (channel * 2)), Duration)).ToArray();
}