namespace MrKWatkins.OakIO.Tape.Sounds;

internal sealed class Pulse(int lengthInTStates) : Sound
{
    private bool signal;

    public int LengthInTStates => lengthInTStates;

    // ReSharper disable once InconsistentNaming
    public int TStatesRemaining { get; private set; }

    public override bool Signal => signal;

    // ReSharper disable once ParameterHidesMember
    public override void Start(bool signal)
    {
        this.signal = signal;
        TStatesRemaining = LengthInTStates;
    }

    public override int Advance(int tStates)
    {
        if (tStates <= TStatesRemaining)
        {
            TStatesRemaining -= tStates;
            return 0;
        }

        var tStatesLeftOver = tStates - TStatesRemaining;
        TStatesRemaining = 0;
        return tStatesLeftOver;
    }

    public override string ToString() => $"P:{lengthInTStates}";
}