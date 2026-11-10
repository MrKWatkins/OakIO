namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>A derived playback frame containing ordinary registers and per-channel SID state.</summary>
public sealed class AkyFrame
{
    internal AkyFrame(byte[] registers, IReadOnlyList<bool> retriggers, IReadOnlyList<AkySidState?> sidStates)
    {
        Registers = registers;
        EnvelopeRetriggers = retriggers;
        SidStates = sidStates;
    }

    /// <summary>Gets chip-major register snapshots, including envelope shape.</summary>
    public IReadOnlyList<byte> Registers { get; }

    /// <summary>Gets whether each chip explicitly retriggers its envelope on this frame.</summary>
    public IReadOnlyList<bool> EnvelopeRetriggers { get; }

    /// <summary>Gets active SID modulation in encoded channel order, with null for disabled channels.</summary>
    /// <remarks>These immutable snapshots persist across differential packets until disabled or reset by a new block.</remarks>
    public IReadOnlyList<AkySidState?> SidStates { get; }
}