namespace MrKWatkins.OakIO.Tape.Sounds;

internal sealed class SoundSequence : Sound
{
    private int index;

    internal SoundSequence([InstantHandle] IEnumerable<Sound> sounds)
        : this(sounds.ToList())
    {
    }

    public SoundSequence(params IReadOnlyList<Sound> sounds)
    {
        if (sounds.Count == 0)
        {
            throw new ArgumentException("Value is empty.", nameof(sounds));
        }
        Sounds = sounds;
    }

    public override void Start(bool startSignal)
    {
        index = 0;
        CurrentSound.Start(startSignal);
    }

    public IReadOnlyList<Sound> Sounds { get; }

    public Sound CurrentSound => Sounds[index];

    public override bool Signal => CurrentSound.Signal;

    public override int Advance(int tStates)
    {
        var sound = CurrentSound;
        var tStatesLeftOver = sound.Advance(tStates);
        if (tStatesLeftOver == 0)
        {
            return 0;
        }

        // Sound finished. Was that the last one?
        if (index == Sounds.Count - 1)
        {
            // Yes, we're done.
            return tStatesLeftOver;
        }

        index++;
        CurrentSound.Start(!sound.Signal);
        return Advance(tStatesLeftOver);
    }

    public override string ToString() => $"[{string.Join(", ", Sounds)}]";
}