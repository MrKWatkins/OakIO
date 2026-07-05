using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// An input recording block (0x80) of an RZX file, containing the recorded input frames.
/// </summary>
public sealed class RzxInputRecordingBlock : RzxBlock
{
    private const int MinimumDataLength = 13;

    /// <summary>
    /// Initializes a new instance of the <see cref="RzxInputRecordingBlock" /> class containing uncompressed, unprotected frames.
    /// </summary>
    /// <param name="frames">The input frames.</param>
    /// <param name="startTStates">The T-states counter at the beginning of the recording.</param>
    /// <param name="flags">The recording flags. Only <see cref="RzxInputRecordingFlags.None" /> is supported for writing.</param>
    public RzxInputRecordingBlock(IReadOnlyList<RzxInputFrame> frames, uint startTStates = 0, RzxInputRecordingFlags flags = RzxInputRecordingFlags.None)
        : this(new RzxBlockHeader(RzxBlockType.InputRecording, GetDataLength(frames)), CreateData(frames, startTStates, flags))
    {
    }

    internal RzxInputRecordingBlock(RzxBlockHeader header, byte[] data)
        : base(header, data)
    {
        if (data.Length < MinimumDataLength)
        {
            throw new InvalidDataException("RZX input recording block is too short.");
        }

        var frameCount = GetUInt32(0);
        if (GetByte(4) != 0)
        {
            throw new InvalidDataException("RZX input recording block reserved byte must be zero.");
        }

        StartTStates = GetUInt32(5);

        if (Flags.HasFlag(RzxInputRecordingFlags.Protected))
        {
            throw new NotSupportedException("Protected RZX input recording blocks are not supported.");
        }

        var frameData = AsReadOnlySpan(MinimumDataLength).ToArray();
        if (Flags.HasFlag(RzxInputRecordingFlags.Compressed))
        {
            frameData = ZLib.Decompress(frameData);
        }

        var frames = new RzxInputFrame[frameCount];
        var offset = 0;
        for (var f = 0; f < frames.Length; f++)
        {
            frames[f] = RzxInputFrame.Read(frameData, ref offset);
        }

        Frames = frames;
    }

    /// <summary>
    /// Gets the T-states counter at the beginning of the recording.
    /// </summary>
    public uint StartTStates { get; }

    /// <summary>
    /// Gets the recording flags.
    /// </summary>
    public RzxInputRecordingFlags Flags => (RzxInputRecordingFlags)GetUInt32(9);

    /// <summary>
    /// Gets the input frames.
    /// </summary>
    public IReadOnlyList<RzxInputFrame> Frames { get; }

    [Pure]
    private static uint GetDataLength(IReadOnlyList<RzxInputFrame> frames)
    {
        ArgumentNullException.ThrowIfNull(frames);
        return checked((uint)(MinimumDataLength + frames.Sum(frame => frame.DataLength)));
    }

    [Pure]
    private static byte[] CreateData(IReadOnlyList<RzxInputFrame> frames, uint startTStates, RzxInputRecordingFlags flags)
    {
        ArgumentNullException.ThrowIfNull(frames);
        if (flags != RzxInputRecordingFlags.None)
        {
            throw new InvalidDataException("Only uncompressed unprotected RZX input recording blocks can be written.");
        }

        var body = new byte[GetDataLength(frames)];
        body.SetUInt32(0, checked((uint)frames.Count));
        body[4] = 0;
        body.SetUInt32(5, startTStates);
        body.SetUInt32(9, (uint)flags);

        var offset = MinimumDataLength;
        foreach (var frame in frames)
        {
            frame.Write(body, ref offset);
        }

        return body;
    }
}