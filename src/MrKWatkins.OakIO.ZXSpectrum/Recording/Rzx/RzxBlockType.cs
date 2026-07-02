namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// The type of a block in an RZX file, as identified by its block ID byte.
/// </summary>
#pragma warning disable CA1028
public enum RzxBlockType : byte
{
    /// <summary>
    /// No block / an unset block ID.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// A creator information block (0x10) describing the program that created the file.
    /// </summary>
    Creator = 0x10,

    /// <summary>
    /// A security information block (0x20). Not supported.
    /// </summary>
    SecurityInformation = 0x20,

    /// <summary>
    /// A security signature block (0x21). Not supported.
    /// </summary>
    SecuritySignature = 0x21,

    /// <summary>
    /// A snapshot block (0x30) containing the machine state at the start of a recording.
    /// </summary>
    Snapshot = 0x30,

    /// <summary>
    /// An input recording block (0x80) containing the recorded input frames.
    /// </summary>
    InputRecording = 0x80
}
#pragma warning restore CA1028