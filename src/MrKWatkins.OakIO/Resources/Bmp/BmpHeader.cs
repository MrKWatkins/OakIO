namespace MrKWatkins.OakIO.Resources.Bmp;

/// <summary>
/// The 14-byte BMP file header, retaining its original bytes.
/// </summary>
public sealed class BmpHeader : Header
{
    internal BmpHeader(byte[] data) : base(data)
    {
    }

    /// <summary>
    /// Gets the file signature.
    /// </summary>
    public string Signature => GetString(0, 2);

    /// <summary>
    /// Gets the declared file size in bytes.
    /// </summary>
    public uint FileSize => GetUInt32(2);

    /// <summary>
    /// Gets the first reserved field.
    /// </summary>
    public ushort Reserved1 => GetUInt16(6);

    /// <summary>
    /// Gets the second reserved field.
    /// </summary>
    public ushort Reserved2 => GetUInt16(8);

    /// <summary>
    /// Gets the offset to the pixel data from the start of the file.
    /// </summary>
    public uint PixelDataOffset => GetUInt32(10);

}