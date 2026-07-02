using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Tape.Sounds;

namespace MrKWatkins.OakIO.Tape;

/// <summary>
/// A tape block containing data encoded as a sequence of bit sounds.
/// </summary>
public sealed class DataBlock : TapeBlock
{
    private readonly byte[] data;
    private readonly Sound zeroBitSound;
    private readonly Sound oneBitSound;
    private readonly Sound? tailPulse;
    private Sound currentSound;
    private readonly int lastBitOfLastByte;
    private int currentByte;
    private int currentBit;

    internal DataBlock(IReadOnlyList<byte> data)
        : this(data, Sound.StandardZeroBit(), Sound.StandardOneBit(), 945, initialSignal: true)
    {
    }

    internal DataBlock(IReadOnlyList<byte> data, Sound zeroBitSound, Sound oneBitSound, int lengthOfTailPulse, int usedBitsInLastByte = 8, bool? initialSignal = null)
        : base(initialSignal)
    {
        this.data = data as byte[] ?? data.ToArray();
        this.zeroBitSound = zeroBitSound;
        this.oneBitSound = oneBitSound;
        tailPulse = lengthOfTailPulse > 0 ? new Pulse(lengthOfTailPulse) : null;
        currentSound = zeroBitSound;
        lastBitOfLastByte = 8 - usedBitsInLastByte;
    }

    [Pure]
    internal static DataBlock Create(IReadOnlyList<byte> data) => new(data);

    [Pure]
    internal static DataBlock Create(IReadOnlyList<byte> data, Sound zeroBitSound, Sound oneBitSound, int lengthOfTailPulse, int usedBitsInLastByte = 8, bool? initialSignal = null) =>
        new(data, zeroBitSound, oneBitSound, lengthOfTailPulse, usedBitsInLastByte, initialSignal);

    /// <summary>
    /// Gets the data bytes in the block.
    /// </summary>
    public IReadOnlyList<byte> Data => data;

    /// <summary>
    /// Gets the length of the data in bytes.
    /// </summary>
    public int DataLength => data.Length;

    internal override bool Signal => currentSound.Signal;

    internal override void Start(bool signal)
    {
        currentByte = 0;
        currentBit = 7;
        StartBit(signal);
    }

    private void StartBit(bool signal)
    {
        var bit = data[currentByte].GetBit(currentBit);
        currentSound = bit ? oneBitSound : zeroBitSound;
        currentSound.Start(signal);
    }

    [MustUseReturnValue]
    internal override int Advance(int tStates)
    {
        var tStatesLeftOver = currentSound.Advance(tStates);
        if (tStatesLeftOver == 0)
        {
            return 0;
        }

        // We've finished the current sound. Are we playing the tail?
        if (currentSound == tailPulse)
        {
            // Yes, we're done.
            return tStatesLeftOver;
        }

        // Have we finished the current byte?
        var isLastByte = currentByte == data.Length - 1;
        if ((isLastByte && currentBit == lastBitOfLastByte) || currentBit == 0)
        {
            // Yes, advance the byte.
            currentByte++;

            // Have we finished the last byte?
            if (isLastByte)
            {
                // Yes. Move to the tail.
                if (tailPulse == null)
                {
                    // No tail, we're done.
                    return tStatesLeftOver;
                }

                tailPulse.Start(!Signal);
                currentSound = tailPulse;
                return Advance(tStatesLeftOver);
            }

            currentBit = 7;
        }
        else
        {
            // No, advance the bit.
            currentBit--;
        }

        // Start the new bit.
        StartBit(!currentSound.Signal);
        return Advance(tStatesLeftOver);
    }
}