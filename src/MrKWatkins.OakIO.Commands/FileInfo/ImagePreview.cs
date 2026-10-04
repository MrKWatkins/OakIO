using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Commands.FileInfo;

/// <summary>
/// Row-major RGBA pixels for a browser preview. JSON encodes the bytes as base64.
/// </summary>
public sealed record ImagePreview(int Width, int Height, byte[] Pixels)
{
    [Pure]
    internal static ImagePreview Create(ImageData image)
    {
        var pixels = new byte[checked(image.PixelCount * 4)];
        var offset = 0;
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var colour = image.GetPixel(x, y);
                pixels[offset++] = colour.Red;
                pixels[offset++] = colour.Green;
                pixels[offset++] = colour.Blue;
                pixels[offset++] = colour.Alpha;
            }
        }
        return new ImagePreview(image.Width, image.Height, pixels);
    }
}