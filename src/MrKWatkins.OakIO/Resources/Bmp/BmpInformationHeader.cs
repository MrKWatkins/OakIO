namespace MrKWatkins.OakIO.Resources.Bmp;

/// <summary>
/// The 40-byte BITMAPINFOHEADER, retaining its original bytes and signed height.
/// </summary>
public sealed class BmpInformationHeader : Header
{
    internal BmpInformationHeader(byte[] data) : base(data)
    {
    }

    /// <summary>
    /// Gets the information header size.
    /// </summary>
    public uint Size => GetUInt32(0);

    /// <summary>
    /// Gets the image width.
    /// </summary>
    public int Width => GetInt32(4);

    /// <summary>
    /// Gets the signed image height; negative values indicate top-down storage.
    /// </summary>
    public int Height => GetInt32(8);

    /// <summary>
    /// Gets the number of colour planes.
    /// </summary>
    public ushort Planes => GetUInt16(12);

    /// <summary>
    /// Gets the stored pixel depth.
    /// </summary>
    public ushort BitsPerPixel => GetUInt16(14);

    /// <summary>
    /// Gets the compression identifier; zero denotes uncompressed BI_RGB.
    /// </summary>
    public uint Compression => GetUInt32(16);

    /// <summary>
    /// Gets the declared pixel data size, which may be zero for BI_RGB.
    /// </summary>
    public uint ImageSize => GetUInt32(20);

    /// <summary>
    /// Gets the horizontal resolution in pixels per metre.
    /// </summary>
    public int HorizontalPixelsPerMetre => GetInt32(24);

    /// <summary>
    /// Gets the vertical resolution in pixels per metre.
    /// </summary>
    public int VerticalPixelsPerMetre => GetInt32(28);

    /// <summary>
    /// Gets the declared colour count; zero uses the depth's full palette.
    /// </summary>
    public uint ColoursUsed => GetUInt32(32);

    /// <summary>
    /// Gets the number of important colours.
    /// </summary>
    public uint ImportantColours => GetUInt32(36);

    /// <summary>
    /// Gets whether rows are stored top-down.
    /// </summary>
    public bool IsTopDown => Height < 0;

    /// <summary>
    /// Gets the stored row size, including four-byte alignment padding.
    /// </summary>
    public int RowStride => checked((int)(((long)Width * BitsPerPixel + 31) / 32 * 4));

}