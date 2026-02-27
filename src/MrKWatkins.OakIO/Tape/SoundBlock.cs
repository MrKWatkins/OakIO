using MrKWatkins.OakIO.Tape.Sounds;

namespace MrKWatkins.OakIO.Tape;

/// <summary>
/// A tape block that plays a single sound.
/// </summary>
/// <param name="sound">The sound to play.</param>
/// <param name="initialSignal">The initial signal level, or <c>null</c> to continue from the previous block.</param>
public sealed class SoundBlock(Sound sound, bool? initialSignal = null) : TapeBlock(initialSignal)
{
    /// <summary>
    /// Gets the sound to play.
    /// </summary>
    public Sound Sound { get; } = sound;

    internal override bool Signal => Sound.Signal;

    internal override void Start(bool signal) => Sound.Start(signal);

    internal override int Advance(int tStates) => Sound.Advance(tStates);
}