namespace MrKWatkins.OakIO.Resources.Bmp;

/// <summary>
/// Converts an image file to BMP, preserving colours and palette indices without quantization.
/// </summary>
/// <param name="sourceFormat">The source image format.</param>
public sealed class ImageToBmpConverter(ImageFormat sourceFormat)
    : IOFileConverter<ImageFile, BmpFile>(sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)), BmpFormat.Instance)
{
    /// <inheritdoc />
    [Pure]
    public override BmpFile Convert(ImageFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var image = source.Image;
        if (image is IndexedImageData indexed)
        {
            // BMP has no two-bit variant; promote one/two-bit data while preserving the original indices and palette.
            return new BmpFile(indexed.BitsPerPixel < 4
                ? new IndexedImageData(indexed.Width, indexed.Height, [.. indexed.Pixels], indexed.Palette)
                : indexed);
        }
        if (image is ColourImageData colourImage)
        {
            return new BmpFile(colourImage);
        }

        var pixels = new Colour[image.PixelCount];
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                pixels[y * image.Width + x] = image.GetPixel(x, y);
            }
        }
        return new BmpFile(new ColourImageData(image.Width, image.Height, pixels));
    }
}