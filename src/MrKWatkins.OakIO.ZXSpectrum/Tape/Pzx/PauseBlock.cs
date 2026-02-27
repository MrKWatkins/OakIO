namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// A PZX block representing a pause in the tape.
/// </summary>
public sealed class PauseBlock : PzxBlock<PauseHeader>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="PauseBlock" /> class from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public PauseBlock(Stream stream) : base(new PauseHeader(stream), stream)
    {
    }

    internal PauseBlock(byte[] headerData) : base(new PauseHeader(headerData), [])
    {
    }

    /// <inheritdoc />
    public override string ToString() => $"Pause: Initial Level = {(Header.InitialPulseLevel ? 1 : 0)}, Duration = {Header.Duration},";
}