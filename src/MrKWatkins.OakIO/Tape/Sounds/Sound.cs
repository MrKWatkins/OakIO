namespace MrKWatkins.OakIO.Tape.Sounds;

/// <summary>
/// Base class for a sound that can be played on a tape.
/// </summary>
public abstract class Sound
{
    /// <summary>
    /// Creates a pure tone sound consisting of alternating pulses.
    /// </summary>
    /// <param name="repeats">The number of pulse repetitions.</param>
    /// <param name="pulseLengthInTStates">The length of each pulse in T-states.</param>
    /// <returns>A new pure tone <see cref="Sound" />.</returns>
    [Pure]
    public static Sound PureTone(int repeats, int pulseLengthInTStates) => new AlternatingSound(repeats, new Pulse(pulseLengthInTStates));

    /// <summary>
    /// Creates a pure tone sound followed by sync pulses.
    /// </summary>
    /// <param name="pureToneRepeats">The number of pure tone pulse repetitions.</param>
    /// <param name="pureTonePulseLength">The length of each pure tone pulse in T-states.</param>
    /// <param name="firstSyncPulseLength">The length of the first sync pulse in T-states, or 0 for no first sync pulse.</param>
    /// <param name="secondSyncPulseLength">The length of the second sync pulse in T-states, or 0 for no second sync pulse.</param>
    /// <returns>A new <see cref="Sound" /> consisting of a pure tone followed by sync pulses.</returns>
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

    /// <summary>
    /// Creates the standard header pure tone and sync sound.
    /// </summary>
    /// <returns>A new <see cref="Sound" /> with standard header pure tone and sync timings.</returns>
    [Pure]
    public static Sound StandardHeaderPureToneAndSync() => PureToneAndSync(8063, 2168, 667, 735);

    /// <summary>
    /// Creates the standard data pure tone and sync sound.
    /// </summary>
    /// <returns>A new <see cref="Sound" /> with standard data pure tone and sync timings.</returns>
    [Pure]
    public static Sound StandardDataPureToneAndSync() => PureToneAndSync(3223, 2168, 667, 735);

    /// <summary>
    /// Creates a sound representing a single bit encoded as two pulses of equal length.
    /// </summary>
    /// <param name="pulseLength">The length of each pulse in T-states.</param>
    /// <returns>A new <see cref="Sound" /> representing a single bit.</returns>
    [Pure]
    public static Sound Bit(int pulseLength) => new SoundSequence(new Pulse(pulseLength), new Pulse(pulseLength));

    /// <summary>
    /// Creates the standard zero bit sound.
    /// </summary>
    /// <returns>A new <see cref="Sound" /> representing a standard zero bit.</returns>
    [Pure]
    public static Sound StandardZeroBit() => Bit(855);

    /// <summary>
    /// Creates the standard one bit sound.
    /// </summary>
    /// <returns>A new <see cref="Sound" /> representing a standard one bit.</returns>
    [Pure]
    public static Sound StandardOneBit() => Bit(1710);

    /// <summary>
    /// Creates a sound from a sequence of pulses with the specified lengths.
    /// </summary>
    /// <param name="lengths">The lengths of the pulses in T-states.</param>
    /// <returns>A new <see cref="Sound" /> consisting of the specified pulse sequence.</returns>
    [Pure]
    public static Sound PulseSequence(IEnumerable<ushort> lengths) => new SoundSequence(lengths.Select(l => new Pulse(l)).ToList());

    /// <summary>
    /// Creates a sound from a sequence of pulses with the specified lengths.
    /// </summary>
    /// <param name="lengths">The lengths of the pulses in T-states.</param>
    /// <returns>A new <see cref="Sound" /> consisting of the specified pulse sequence.</returns>
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

    /// <summary>
    /// Gets the current signal level of the sound.
    /// </summary>
    public abstract bool Signal { get; }

    /// <summary>
    /// Starts the sound with the specified initial signal level.
    /// </summary>
    /// <param name="signal">The initial signal level.</param>
    public abstract void Start(bool signal);

    /// <summary>
    /// Advances the sound by <paramref name="tStates" />. Returns 0 if the sound is still in progress, or
    /// the number of T states left over if the sound is finished.
    /// </summary>
    [MustUseReturnValue]
    public abstract int Advance(int tStates);
}