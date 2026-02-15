namespace MrKWatkins.OakIO.Tape.Sounds;

public abstract class Sound
{
    [Pure]
    public static Sound PureTone(int repeats, int pulseLengthInTStates) => new AlternatingSound(repeats, new Pulse(pulseLengthInTStates));

    [Pure]
    public static Sound PureToneAndSync(int pureToneRepeats, int pureTonePulseLength, int firstSyncPulseLength, int secondSyncPulseLength)
    {
        var sounds = new List<Sound>(3) { PureTone(pureToneRepeats, pureTonePulseLength) };
        if (firstSyncPulseLength > 0)
        {
            sounds.Add(new Pulse(firstSyncPulseLength));
        }
        if (secondSyncPulseLength > 0)
        {
            sounds.Add(new Pulse(secondSyncPulseLength));
        }
        return new SoundSequence(sounds);
    }

    [Pure]
    public static Sound StandardHeaderPureToneAndSync() => PureToneAndSync(8063, 2168, 667, 735);

    [Pure]
    public static Sound StandardDataPureToneAndSync() => PureToneAndSync(3223, 2168, 667, 735);

    [Pure]
    public static Sound Bit(int pulseLength) => new SoundSequence(new Pulse(pulseLength), new Pulse(pulseLength));

    [Pure]
    public static Sound StandardZeroBit() => Bit(855);

    [Pure]
    public static Sound StandardOneBit() => Bit(1710);

    [Pure]
    public static Sound PulseSequence(IEnumerable<ushort> lengths) => new SoundSequence(lengths.Select(l => new Pulse(l)).ToList());

    [Pure]
    public static Sound PulseSequence(ReadOnlySpan<ushort> lengths)
    {
        var pulses = new List<Pulse>(lengths.Length);
        foreach (var length in lengths)
        {
            pulses.Add(new Pulse(length));
        }
        return new SoundSequence(pulses);
    }

    public abstract bool Signal { get; }

    public abstract void Start(bool signal);

    /// <summary>
    /// Advances the sound by <paramref name="tStates" />. Returns 0 if the sound is still in progress, or
    /// the number of T states left over if the sound is finished.
    /// </summary>
    [MustUseReturnValue]
    public abstract int Advance(int tStates);
}