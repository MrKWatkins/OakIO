namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

// Names from https://worldofspectrum.net/TZXformat.html.
public enum TzxBlockType
{
    Unknown = 0,
    StandardSpeedData = 0x10,
    TurboSpeedData = 0x11,
    PureTone = 0x12,
    PulseSequence = 0x13,
    PureData = 0x14,
    DirectRecording = 0x15,
    CswRecording = 0x18,
    GeneralizedDataBlock = 0x19,
    Pause = 0x20,
    GroupStart = 0x21,
    GroupEnd = 0x22,
    JumpToBlock = 0x23,
    LoopStart = 0x24,
    LoopEnd = 0x25,
    CallSequence = 0x26,
    ReturnFromSequence = 0x27,
    SelectBlock = 0x28,
    StopTheTapeIf48K = 0x2A,
    SetSignalLevel = 0x2B,
    TextDescription = 0x30,
    MessageBlock = 0x31,
    ArchiveInfo = 0x32,
    HardwareType = 0x33,
    CustomInfoBlock = 0x35,
    GlueBlock = 0x5A
}