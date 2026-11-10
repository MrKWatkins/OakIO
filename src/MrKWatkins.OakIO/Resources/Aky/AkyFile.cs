namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>An AKY music file with byte-backed native components and linked structural views.</summary>
/// <remarks>Absolute exports require their external origin. Origin zero also reads song-relative exports.</remarks>
public sealed class AkyFile : MusicFile
{
    /// <summary>Reads native AKY bytes and validates the header and linked structural references.</summary>
    /// <param name="bytes">The native file bytes.</param>
    /// <param name="format">The pointer interpretation, defaulting to song-relative offsets.</param>
    public AkyFile(byte[] bytes, AkyFormat? format = null) : base(format ?? AkyFormat.Instance)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length < 6 || bytes.Length > 65536 - Format.Origin)
        {
            throw new InvalidDataException("An AKY file must have a complete header and fit its sixteen-bit address space.");
        }
        var channelCount = bytes[1];
        if (channelCount == 0)
        {
            throw new InvalidDataException("AKY channel count must be positive.");
        }
        var headerLength = 2 + (channelCount + 2) / 3 * 4;
        if (headerLength > bytes.Length || (bytes[0] & 15) > 1 || (bytes[0] & 64) != 0 || ((bytes[0] >> 4) & 3) == 3)
        {
            throw new InvalidDataException("The AKY header is truncated or uses unsupported version or flag values.");
        }
        Header = new AkyHeader(bytes[..headerLength]);
        Body = new AkyData(bytes[headerLength..]);
        for (var chip = 0; chip < Header.ChipCount; ++chip)
        {
            if (Header.GetClock(chip) == 0)
            {
                throw new InvalidDataException("AKY chip clocks must be positive.");
            }
        }
        var patterns = ReadPatterns();
        foreach (var pattern in patterns)
        {
            foreach (var track in pattern.Tracks)
            {
                _ = track.Blocks;
            }
        }
    }

    /// <summary>Gets the pointer interpretation used by this file.</summary>
    public new AkyFormat Format => (AkyFormat)base.Format;

    /// <summary>Gets the native version, channel count and chip-clock header.</summary>
    public AkyHeader Header { get; }

    /// <summary>Gets the remaining native file bytes.</summary>
    public AkyData Body { get; }

    /// <summary>Gets the linked patterns before the song-loop marker.</summary>
    public IReadOnlyList<AkyPattern> Patterns => ReadPatterns();

    /// <summary>Gets the index of the pattern selected by the song-loop pointer.</summary>
    [Pure]
    public int GetLoopPatternIndex()
    {
        var patterns = ReadPatterns();
        var loopOffset = Header.Length + patterns.Count * (2 + 2 * Header.ChannelCount);
        var destination = ReadPointer(loopOffset + 2);
        for (var index = 0; index < patterns.Count; ++index)
        {
            if (patterns[index].Offset == destination)
            {
                return index;
            }
        }
        throw new InvalidDataException("The AKY loop does not refer to a pattern boundary.");
    }

    /// <summary>Decodes one complete pass up to the song-loop marker.</summary>
    /// <returns>Register snapshots with explicit envelope-retrigger masks.</returns>
    /// <remarks>Truncated packets and invalid control loops are rejected during enumeration.</remarks>
    [Pure]
    public IEnumerable<AkyFrame> DecodeFrames() => AkyDecoder.Decode(this);

    [Pure]
    private IReadOnlyList<AkyPattern> ReadPatterns()
    {
        var patterns = new List<AkyPattern>();
        var offset = Header.Length;
        while (ReadWord(offset) != 0)
        {
            CheckRange(offset, 2 + Header.ChannelCount * 2);
            patterns.Add(new AkyPattern(this, offset));
            offset += 2 + Header.ChannelCount * 2;
        }
        var loop = ReadPointer(offset + 2);
        if (patterns.All(pattern => pattern.Offset != loop))
        {
            throw new InvalidDataException("The AKY loop must refer to an earlier pattern boundary.");
        }
        return patterns;
    }

    internal void CheckRange(int offset, int length)
    {
        if (offset < Header.Length || length < 0 || offset > Header.Length + Body.Length - length)
        {
            throw new InvalidDataException("An AKY reference or packet falls outside the native music data.");
        }
    }

    [Pure]
    internal byte ReadByte(int offset)
    {
        CheckRange(offset, 1);
        return Body.Data[offset - Header.Length];
    }

    [Pure]
    internal ushort ReadWord(int offset)
    {
        CheckRange(offset, 2);
        var first = ReadByte(offset);
        var second = ReadByte(offset + 1);
        return (ushort)(Header.IsLittleEndian ? first | second << 8 : first << 8 | second);
    }

    [Pure]
    internal int ReadPointer(int offset)
    {
        var result = ReadWord(offset) - Format.Origin;
        CheckRange(result, 1);
        return result;
    }
}