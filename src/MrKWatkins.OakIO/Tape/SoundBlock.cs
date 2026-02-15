using MrKWatkins.OakIO.Tape.Sounds;

namespace MrKWatkins.OakIO.Tape;

public sealed class SoundBlock(Sound sound, bool? initialSignal = null) : TapeBlock(initialSignal)
{
    public Sound Sound { get; } = sound;

    internal override bool Signal => Sound.Signal;

    internal override void Start(bool signal) => Sound.Start(signal);

    internal override int Advance(int tStates) => Sound.Advance(tStates);
}
