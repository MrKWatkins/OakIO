using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// A single input frame within an <see cref="RzxInputRecordingBlock" />.
/// </summary>
public sealed class RzxInputFrame
{
    /// <summary>
    /// The value of the IN counter that indicates a repeated frame, that is the port reads are the same as the previous frame.
    /// </summary>
    public const ushort RepeatedInputReads = ushort.MaxValue;

    private readonly byte[] inputReads;

    /// <summary>
    /// Initializes a new instance of the <see cref="RzxInputFrame" /> class.
    /// </summary>
    /// <param name="fetchCount">The number of instruction fetches performed in this frame.</param>
    /// <param name="inputReads">The values returned by the CPU's I/O port reads during this frame, or <c>null</c> for none.</param>
    /// <param name="repeatsPreviousInputReads">Whether the port reads are the same as the previous frame.</param>
    public RzxInputFrame(ushort fetchCount, byte[]? inputReads = null, bool repeatsPreviousInputReads = false)
    {
        if (inputReads is not null && repeatsPreviousInputReads)
        {
            throw new ArgumentException("Repeated frames cannot include input read data.", nameof(inputReads));
        }
        if (inputReads is { Length: > RepeatedInputReads - 1 })
        {
            throw new ArgumentException("RZX input frames support at most 65534 input reads.", nameof(inputReads));
        }

        FetchCount = fetchCount;
        this.inputReads = inputReads ?? [];
        RepeatsPreviousInputReads = repeatsPreviousInputReads;
    }

    /// <summary>
    /// Gets the number of instruction fetches performed in this frame.
    /// </summary>
    public ushort FetchCount { get; }

    /// <summary>
    /// Gets the values returned by the CPU's I/O port reads during this frame. Empty if <see cref="RepeatsPreviousInputReads" /> is <c>true</c>.
    /// </summary>
    public IReadOnlyList<byte> InputReads => inputReads;

    /// <summary>
    /// Gets a value indicating whether the port reads are the same as the previous frame.
    /// </summary>
    public bool RepeatsPreviousInputReads { get; }

    /// <summary>
    /// Gets the length of this frame's data in bytes.
    /// </summary>
    internal uint DataLength => checked((uint)(4 + inputReads.Length));

    [MustUseReturnValue]
    internal static RzxInputFrame Read(byte[] data, ref int offset)
    {
        if (offset + 4 > data.Length)
        {
            throw new InvalidDataException("RZX input frame ended unexpectedly.");
        }

        var fetchCount = data.GetUInt16(offset);
        var inputReadCount = data.GetUInt16(offset + 2);
        offset += 4;

        if (inputReadCount == RepeatedInputReads)
        {
            return new RzxInputFrame(fetchCount, repeatsPreviousInputReads: true);
        }

        if (offset + inputReadCount > data.Length)
        {
            throw new InvalidDataException("RZX input frame ended before all input reads were read.");
        }

        var inputReads = data[offset..(offset + inputReadCount)];
        offset += inputReadCount;
        return new RzxInputFrame(fetchCount, inputReads);
    }

    internal void Write(byte[] target, ref int offset)
    {
        target.SetUInt16(offset, FetchCount);
        target.SetUInt16(offset + 2, RepeatsPreviousInputReads ? RepeatedInputReads : (ushort)inputReads.Length);
        offset += 4;

        if (!RepeatsPreviousInputReads)
        {
            inputReads.CopyTo(target, offset);
            offset += inputReads.Length;
        }
    }
}