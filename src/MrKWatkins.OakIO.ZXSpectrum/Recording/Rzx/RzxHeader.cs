using System.Text;
using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// The global file header for an RZX file.
/// </summary>
public sealed class RzxHeader : Header
{
    private const string ExpectedSignature = "RZX!";

    /// <summary>
    /// The length of the RZX file header in bytes.
    /// </summary>
    internal const int HeaderLength = 10;

    /// <summary>
    /// Initialises a new instance of the <see cref="RzxHeader" /> class for version 0.13.
    /// </summary>
    /// <param name="flags">The header flags.</param>
    public RzxHeader(uint flags = 0)
        : base(CreateData(flags))
    {
    }

    internal RzxHeader(byte[] data)
        : base(data)
    {
    }

    /// <summary>
    /// Gets the four character file signature. Should be <c>RZX!</c> for a valid file.
    /// </summary>
    public string Signature => GetString(0, 4);

    /// <summary>
    /// Gets the RZX major revision number.
    /// </summary>
    public byte MajorVersion => GetByte(4);

    /// <summary>
    /// Gets the RZX minor revision number.
    /// </summary>
    public byte MinorVersion => GetByte(5);

    /// <summary>
    /// Gets the header flags.
    /// </summary>
    public uint Flags => GetUInt32(6);

    /// <summary>
    /// Gets a value indicating whether the <see cref="Signature" /> is valid.
    /// </summary>
    public bool IsValid => Signature == ExpectedSignature;

    /// <summary>
    /// Gets a value indicating whether the file's version is supported.
    /// </summary>
    public bool IsSupportedVersion => MajorVersion == 0 && MinorVersion is 12 or 13;

    [Pure]
    private static byte[] CreateData(uint flags)
    {
        var data = new byte[HeaderLength];
        Encoding.ASCII.GetBytes(ExpectedSignature).CopyTo(data, 0);
        data[4] = 0;
        data[5] = 13;
        data.SetUInt32(6, flags);
        return data;
    }
}