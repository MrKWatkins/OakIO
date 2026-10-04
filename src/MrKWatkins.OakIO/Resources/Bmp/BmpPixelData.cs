namespace MrKWatkins.OakIO.Resources.Bmp;

/// <summary>
/// Stored BMP pixel rows, including packing, orientation and padding.
/// </summary>
public sealed class BmpPixelData : IOFileComponent
{
    internal BmpPixelData(byte[] data) : base(data)
    {
    }

    /// <summary>
    /// Converts the stored rows to top-left-origin, row-major image data.
    /// </summary>

    /// <param name="header">The information header describing the rows.</param>
    /// <param name="palette">The colour table for indexed images, or null for RGB24.</param>
    /// <returns>The normalised image view.</returns>
    [Pure]
    public ImageData GetImage(BmpInformationHeader header, BmpPalette? palette)
    {
        ArgumentNullException.ThrowIfNull(header);
        var width = header.Width;
        var height = Math.Abs(header.Height);
        var stride = header.RowStride;
        if (header.BitsPerPixel == 24)
        {
            var pixels = new Colour[checked(width * height)];
            for (var row = 0; row < height; row++)
            {
                var destination = (header.IsTopDown ? row : height - 1 - row) * width;
                for (var x = 0; x < width; x++)
                {
                    var offset = row * stride + x * 3;
                    pixels[destination + x] = new Colour(GetByte(offset + 2), GetByte(offset + 1), GetByte(offset));
                }
            }
            return new ColourImageData(width, height, pixels);
        }
        ArgumentNullException.ThrowIfNull(palette);
        var indices = new byte[checked(width * height)];
        for (var row = 0; row < height; row++)
        {
            var destination = (header.IsTopDown ? row : height - 1 - row) * width;
            for (var x = 0; x < width; x++)
            {
                var packed = GetByte(row * stride + (header.BitsPerPixel == 8 ? x : x / 2));
                indices[destination + x] = header.BitsPerPixel == 8 ? packed : (byte)(x % 2 == 0 ? packed >> 4 : packed & 15);
            }
        }
        return new IndexedImageData(width, height, indices, new PaletteData(palette.Colours), header.BitsPerPixel);
    }
}