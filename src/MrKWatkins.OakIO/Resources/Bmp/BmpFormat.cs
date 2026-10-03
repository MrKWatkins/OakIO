using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Resources.Bmp;

/// <summary>
/// Reads and writes uncompressed four/eight-bit indexed and RGB24 Windows BMP images.
/// </summary>
public sealed class BmpFormat : ImageFormat<BmpFile>
{
    private const int HeaderSize = 54;
    private const uint InfoHeaderSize = 40;

    /// <summary>
    /// The singleton BMP format.
    /// </summary>
    public static readonly BmpFormat Instance = new();

    private BmpFormat() : base("Windows Bitmap", "bmp")
    {
    }

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        ReadFile(await reader.ReadToEndAsync().ConfigureAwait(false));

    [Pure]
    private static BmpFile ReadFile(byte[] data)
    {
        if (data.Length < 18)
        {
            throw new EndOfStreamException("The BMP file header is truncated.");
        }
        if (!data.AsSpan(0, 2).SequenceEqual("BM"u8))
        {
            throw new InvalidDataException("Not a BMP file: missing BM signature.");
        }
        if (data.GetUInt32(14) != InfoHeaderSize)
        {
            throw new NotSupportedException("Only BMP files with a 40-byte BITMAPINFOHEADER are supported.");
        }
        if (data.Length < HeaderSize)
        {
            throw new EndOfStreamException("The BMP information header is truncated.");
        }
        if (data.GetUInt32(6) != 0 || data.GetUInt16(26) != 1)
        {
            throw new InvalidDataException("BMP reserved fields must be zero and the plane count must be one.");
        }

        var width = data.GetInt32(18);
        var signedHeight = data.GetInt32(22);
        if (width <= 0 || signedHeight is 0 or int.MinValue)
        {
            throw new InvalidDataException("BMP dimensions must have a positive width and a nonzero representable height.");
        }
        var height = Math.Abs(signedHeight);
        var bitsPerPixel = data.GetUInt16(28);
        if (bitsPerPixel is not (4 or 8 or 24))
        {
            throw new NotSupportedException($"BMP pixel depth {bitsPerPixel} is not supported.");
        }
        if (data.GetUInt32(30) != 0)
        {
            throw new NotSupportedException("Only uncompressed BI_RGB BMP files are supported.");
        }

        var stride = GetRowStride(width, bitsPerPixel);
        var pixelCount = (long)width * height;
        if (stride > int.MaxValue || pixelCount > int.MaxValue || stride * height > int.MaxValue)
        {
            throw new InvalidDataException("The BMP dimensions exceed the supported image size.");
        }
        var pixelBytes = (int)(stride * height);
        int colourCount;
        if (bitsPerPixel != 24)
        {
            var declaredColours = data.GetUInt32(46);
            var maximumColours = 1 << bitsPerPixel;
            if (declaredColours > maximumColours)
            {
                throw new InvalidDataException("The BMP palette is too large for its pixel depth.");
            }
            colourCount = declaredColours == 0 ? maximumColours : (int)declaredColours;
        }
        else
        {
            var declaredColours = data.GetUInt32(46);
            if (declaredColours > (int.MaxValue - HeaderSize) / 4)
            {
                throw new InvalidDataException("The BMP colour table exceeds the supported size.");
            }
            colourCount = (int)declaredColours;
        }

        var pixelOffset = data.GetUInt32(10);
        if (pixelOffset < HeaderSize + colourCount * 4)
        {
            throw new InvalidDataException("The BMP pixel offset overlaps the header or palette.");
        }
        var pixelEnd = pixelOffset + pixelBytes;
        var declaredSize = data.GetUInt32(2);
        if (pixelEnd > data.Length || declaredSize > data.Length)
        {
            throw new EndOfStreamException("The BMP palette or pixel data is truncated.");
        }
        if (declaredSize < pixelEnd)
        {
            throw new InvalidDataException("The BMP file size does not include all pixel data.");
        }
        var declaredPixelBytes = data.GetUInt32(34);
        if (declaredPixelBytes != 0 && declaredPixelBytes != pixelBytes)
        {
            throw new InvalidDataException("The BMP image size does not match its dimensions.");
        }

        if (bitsPerPixel != 24)
        {
            for (var row = 0; row < height; row++)
            {
                var source = (int)(pixelOffset + row * stride);
                for (var x = 0; x < width; x++)
                {
                    var packed = data[source + (bitsPerPixel == 8 ? x : x / 2)];
                    var index = bitsPerPixel == 8 ? packed : x % 2 == 0 ? packed >> 4 : packed & 15;
                    if (index >= colourCount)
                    {
                        throw new InvalidDataException("A BMP pixel refers to a missing palette entry.");
                    }
                }
            }
        }
        return new BmpFile(data);
    }

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(BmpFile file, IBinaryWriter writer)
    {
        await file.Header.WriteAsync(writer).ConfigureAwait(false);
        await file.InformationHeader.WriteAsync(writer).ConfigureAwait(false);
        if (file.Palette != null)
        {
            await file.Palette.WriteAsync(writer).ConfigureAwait(false);
        }
        await writer.WriteAsync(file.GapMemory).ConfigureAwait(false);
        await file.PixelData.WriteAsync(writer).ConfigureAwait(false);
        await writer.WriteAsync(file.TrailingDataMemory).ConfigureAwait(false);
    }

    [Pure]
    internal static byte[] CreateBytes(ImageData image, bool isTopDown)
    {
        var bitsPerPixel = BmpFile.ValidateImage(image);
        var indexed = image as IndexedImageData;
        var colourCount = indexed?.Palette.Colours.Count ?? 0;
        var pixelOffset = HeaderSize + colourCount * 4;
        var stride = checked((int)GetRowStride(image.Width, bitsPerPixel));
        var pixelBytes = checked(stride * image.Height);
        var data = new byte[checked(pixelOffset + pixelBytes)];
        "BM"u8.CopyTo(data);
        data.SetUInt32(2, (uint)data.Length);
        data.SetUInt32(10, (uint)pixelOffset);
        data.SetUInt32(14, InfoHeaderSize);
        data.SetInt32(18, image.Width);
        data.SetInt32(22, isTopDown ? -image.Height : image.Height);
        data.SetUInt16(26, 1);
        data.SetUInt16(28, (ushort)bitsPerPixel);
        data.SetUInt32(34, (uint)pixelBytes);
        data.SetUInt32(46, (uint)colourCount);

        if (indexed != null)
        {
            for (var i = 0; i < colourCount; i++)
            {
                var colour = indexed.Palette.Colours[i];
                var offset = HeaderSize + i * 4;
                data[offset] = colour.Blue;
                data[offset + 1] = colour.Green;
                data[offset + 2] = colour.Red;
            }
        }
        for (var row = 0; row < image.Height; row++)
        {
            var y = isTopDown ? row : image.Height - 1 - row;
            var destination = pixelOffset + row * stride;
            for (var x = 0; x < image.Width; x++)
            {
                if (indexed == null)
                {
                    var colour = image.GetPixel(x, y);
                    var offset = destination + x * 3;
                    data[offset] = colour.Blue;
                    data[offset + 1] = colour.Green;
                    data[offset + 2] = colour.Red;
                }
                else
                {
                    var index = indexed.Pixels[y * image.Width + x];
                    if (bitsPerPixel == 8)
                    {
                        data[destination + x] = index;
                    }
                    else
                    {
                        data[destination + x / 2] |= (byte)(x % 2 == 0 ? index << 4 : index);
                    }
                }
            }
        }
        return data;
    }

    [Pure]
    private static long GetRowStride(int width, int bitsPerPixel) => ((long)width * bitsPerPixel + 31) / 32 * 4;
}