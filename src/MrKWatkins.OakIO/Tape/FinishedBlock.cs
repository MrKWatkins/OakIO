namespace MrKWatkins.OakIO.Tape;

internal sealed class FinishedBlock : TapeBlock
{
    private bool signal;

    internal override bool Signal => signal;

    // ReSharper disable once ParameterHidesMember
    internal override void Start(bool signal) => this.signal = signal;

    internal override int Advance(int _) => 0;
}
