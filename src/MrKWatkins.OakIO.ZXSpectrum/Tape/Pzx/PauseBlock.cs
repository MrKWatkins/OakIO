namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// A PZX block representing a pause in the tape.
/// </summary>
public sealed class PauseBlock : PzxBlock<PauseHeader>
{
    internal PauseBlock(byte[] headerData) : base(new PauseHeader(headerData), [])
    {
    }

    /// <inheritdoc />
    public override string ToString() => $"Pause: Initial Level = {(Header.InitialPulseLevel ? 1 : 0)}, Duration = {Header.Duration},";
}