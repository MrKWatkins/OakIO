using MrKWatkins.OakIO.Tape.Sounds;

namespace MrKWatkins.OakIO.Tape;

public sealed class PauseBlock(int lengthInTStates, bool? initialSignal = null) : TapeBlock(initialSignal)
{
    private readonly Pulse pulse = new(lengthInTStates);

    public int LengthInTStates { get; } = lengthInTStates;

    internal override bool Signal => pulse.Signal;

    internal override void Start(bool signal) => pulse.Start(signal);

    internal override int Advance(int tStates) => pulse.Advance(tStates);

    [Pure]
    public static PauseBlock OneSecond(decimal tStatesPerSecond) => new((int)tStatesPerSecond);
}
