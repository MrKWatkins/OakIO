namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

// Names from https://worldofspectrum.net/TZXformat.html.
/// <summary>
/// The types of blocks that can appear in a TZX tape file.
/// </summary>
public enum TzxBlockType
{
    /// <summary>
    /// An unknown or unrecognized block type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Standard speed data block as used by the ROM loading routines.
    /// </summary>
    StandardSpeedData = 0x10,

    /// <summary>
    /// Turbo speed data block with custom timing parameters.
    /// </summary>
    TurboSpeedData = 0x11,

    /// <summary>
    /// Pure tone block consisting of identical pulses.
    /// </summary>
    PureTone = 0x12,

    /// <summary>
    /// Sequence of pulses with arbitrary durations.
    /// </summary>
    PulseSequence = 0x13,

    /// <summary>
    /// Pure data block with no pilot tone.
    /// </summary>
    PureData = 0x14,

    /// <summary>
    /// Direct recording of audio samples.
    /// </summary>
    DirectRecording = 0x15,

    /// <summary>
    /// CSW compressed audio recording.
    /// </summary>
    CswRecording = 0x18,

    /// <summary>
    /// Generalized data block with custom pulse sequences.
    /// </summary>
    GeneralizedDataBlock = 0x19,

    /// <summary>
    /// Pause or silence in the tape.
    /// </summary>
    Pause = 0x20,

    /// <summary>
    /// Start of a group of blocks.
    /// </summary>
    GroupStart = 0x21,

    /// <summary>
    /// End of a group of blocks.
    /// </summary>
    GroupEnd = 0x22,

    /// <summary>
    /// Jump to another block in the file.
    /// </summary>
    JumpToBlock = 0x23,

    /// <summary>
    /// Start of a loop.
    /// </summary>
    LoopStart = 0x24,

    /// <summary>
    /// End of a loop.
    /// </summary>
    LoopEnd = 0x25,

    /// <summary>
    /// Call a sequence of blocks.
    /// </summary>
    CallSequence = 0x26,

    /// <summary>
    /// Return from a call sequence.
    /// </summary>
    ReturnFromSequence = 0x27,

    /// <summary>
    /// Select one of several blocks.
    /// </summary>
    SelectBlock = 0x28,

    /// <summary>
    /// Stop the tape if the machine is a 48K ZX Spectrum.
    /// </summary>
    StopTheTapeIf48K = 0x2A,

    /// <summary>
    /// Set the signal level for the next block.
    /// </summary>
    SetSignalLevel = 0x2B,

    /// <summary>
    /// Free-form text description of the tape.
    /// </summary>
    TextDescription = 0x30,

    /// <summary>
    /// Message block displayed during playback.
    /// </summary>
    MessageBlock = 0x31,

    /// <summary>
    /// Archive info block containing tape metadata.
    /// </summary>
    ArchiveInfo = 0x32,

    /// <summary>
    /// Hardware type information block.
    /// </summary>
    HardwareType = 0x33,

    /// <summary>
    /// Custom information block.
    /// </summary>
    CustomInfoBlock = 0x35,

    /// <summary>
    /// Glue block used to splice TZX files together.
    /// </summary>
    GlueBlock = 0x5A
}