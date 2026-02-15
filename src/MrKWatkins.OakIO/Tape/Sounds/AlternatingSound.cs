namespace MrKWatkins.OakIO.Tape.Sounds;

internal sealed class AlternatingSound : Sound
{
    public AlternatingSound(int repeats, Sound sound)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(repeats);
        Repeats = repeats;
        Sound = sound;
    }

    public int Repeats { get; }

    public Sound Sound { get; }

    public int RepeatsRemaining { get; private set; }

    public override void Start(bool startSignal)
    {
        Sound.Start(startSignal);
        RepeatsRemaining = Repeats - 1;
    }

    public override bool Signal => Sound.Signal;

    public override int Advance(int tStates)
    {
        var tStatesLeftOver = Sound.Advance(tStates);
        if (tStatesLeftOver == 0)
        {
            return 0;
        }

        // Pulse finished. Was that the last one?
        if (RepeatsRemaining == 0)
        {
            // Yes, we're done.
            return tStatesLeftOver;
        }

        RepeatsRemaining--;
        Sound.Start(!Sound.Signal);
        return Sound.Advance(tStatesLeftOver);
    }

    public override string ToString() => $"{Repeats} x {Sound}";
}