using MrKWatkins.OakIO.Tape.Sounds;

namespace MrKWatkins.OakIO.Tape;

/// <summary>
/// A tape block representing a pause of a specified duration.
/// </summary>
/// <param name="lengthInTStates">The length of the pause in T-states.</param>
/// <param name="initialSignal">The initial signal level, or <c>null</c> to continue from the previous block.</param>
public sealed class PauseBlock(int lengthInTStates, bool? initialSignal = null) : TapeBlock(initialSignal)
{
    private readonly Pulse pulse = new(lengthInTStates);

    /// <summary>
    /// Gets the length of the pause in T-states.
    /// </summary>
    public int LengthInTStates { get; } = lengthInTStates;

    internal override bool Signal => pulse.Signal;

    internal override void Start(bool signal) => pulse.Start(signal);

    internal override int Advance(int tStates) => pulse.Advance(tStates);

    /// <summary>
    /// Creates a one second pause block.
    /// </summary>
    /// <param name="tStatesPerSecond">The number of T-states per second.</param>
    /// <returns>A new <see cref="PauseBlock" /> lasting one second.</returns>
    [Pure]
    public static PauseBlock OneSecond(decimal tStatesPerSecond) => new((int)tStatesPerSecond);
}