namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// A signal pulse with a repeat count and duration in T-states.
/// </summary>
/// <param name="Count">The number of times this pulse repeats.</param>
/// <param name="Duration">The duration of the pulse in T-states.</param>
public readonly record struct Pulse(ushort Count, uint Duration)
{
    /// <inheritdoc />
    public override string ToString() => $"{Count} x {Duration}";
}