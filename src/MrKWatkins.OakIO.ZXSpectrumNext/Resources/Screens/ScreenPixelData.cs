using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;

/// <summary>
/// Native SL2/SLR bitmap bytes with a derived unpacked row-major view.
/// </summary>
/// <remarks>
/// https://wiki.specnext.dev/Layer_2
/// https://wiki.specnext.dev/Video_Modes
/// </remarks>
public sealed class ScreenPixelData : IOFileComponent
{
    internal ScreenPixelData(byte[] bytes, int width, int height, int bits) : base(width * height * bits / 8, bytes)
    {
        Width = width;
        Height = height;
        BitsPerPixel = bits;
    }

    /// <summary>
    /// Gets the width supplied by the screen mode.
    /// </summary>
    public int Width { get; }
    /// <summary>
    /// Gets the height supplied by the screen mode.
    /// </summary>
    public int Height { get; }
    /// <summary>
    /// Gets the native bit depth.
    /// </summary>
    public int BitsPerPixel { get; }
    /// <summary>
    /// Gets unpacked row-major pixel indices.
    /// </summary>
    public IReadOnlyList<byte> Pixels
    {
        get
        {
            var pixels = new byte[Width * Height];
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var packed = Data[GetOffset(x, y)];
                    pixels[y * Width + x] = BitsPerPixel == 8 ? packed : (byte)((x % 2 == 0 ? packed >> 4 : packed) & 15);
                }
            }
            return pixels;
        }
    }

    [Pure]
    private int GetOffset(int x, int y) => Width is 320 or 640
        ? x / (8 / BitsPerPixel) * Height + y
        : (y * Width + x) / (8 / BitsPerPixel);

    [Pure]
    internal static ScreenPixelData Create(IndexedImageData image, int width, int height, int bits)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (image.Width != width || image.Height != height)
        {
            throw new ArgumentException("The image dimensions do not match the selected screen mode.", nameof(image));
        }
        var result = new ScreenPixelData(new byte[width * height * bits / 8], width, height, bits);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixel = image.Pixels[y * width + x];
                if (pixel >= 1 << bits)
                {
                    throw new ArgumentException("A pixel index does not fit the native screen depth.", nameof(image));
                }
                var offset = result.GetOffset(x, y);
                result.SetByte(offset, (byte)(result.Data[offset] | (bits == 4 && x % 2 == 0 ? pixel << 4 : pixel)));
            }
        }
        return result;
    }
}