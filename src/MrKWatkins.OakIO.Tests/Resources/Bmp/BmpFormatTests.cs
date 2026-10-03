using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.Tests.Resources.Bmp;

public sealed class BmpFormatTests
{
    [TestCase(4, false)]
    [TestCase(4, true)]
    [TestCase(8, false)]
    [TestCase(8, true)]
    [TestCase(24, false)]
    [TestCase(24, true)]
    public void Read(int depth, bool topDown)
    {
        var file = BmpFormat.Instance.Read(Create(depth, topDown));
        file.BitsPerPixel.Should().Equal(depth);
        file.IsTopDown.Should().Equal(topDown);
        file.Image.Width.Should().Equal(3);
        file.Image.Height.Should().Equal(2);
        for (var y = 0; y < 2; y++)
        {
            for (var x = 0; x < 3; x++)
            {
                file.Image.GetPixel(x, y).Should().Equal(new Colour((byte)(x + y * 3), 10, 20));
            }
        }
    }

    [TestCase(4, false)]
    [TestCase(4, true)]
    [TestCase(8, false)]
    [TestCase(8, true)]
    [TestCase(24, false)]
    [TestCase(24, true)]
    public void Write(int depth, bool topDown)
    {
        var colours = Enumerable.Range(0, 6).Select(i => new Colour((byte)i, 10, 20)).ToArray();
        ImageData image = depth == 24
            ? new ColourImageData(3, 2, colours)
            : new IndexedImageData(3, 2, [0, 1, 2, 3, 4, 5], new PaletteData(colours), depth);
        new BmpFile(image, topDown).ToByteArray().Should().SequenceEqual(Create(depth, topDown));
    }

    [TestCase(0)]
    [TestCase(17)]
    [TestCase(18)]
    [TestCase(53)]
    [TestCase(54)]
    [TestCase(77)]
    public void Read_Truncated(int length) =>
        AssertThat.Invoking(() => BmpFormat.Instance.Read(Create(24, false)[..length])).Should().Throw<EndOfStreamException>();

    [TestCase(14, 12u)]
    [TestCase(14, 108u)]
    [TestCase(28, 1u)]
    [TestCase(28, 16u)]
    [TestCase(28, 32u)]
    [TestCase(30, 1u)]
    [TestCase(30, 3u)]
    public void Read_Unsupported(int offset, uint value)
    {
        var bytes = Create(24, false);
        Set(bytes, offset, value);
        AssertThat.Invoking(() => BmpFormat.Instance.Read(bytes)).Should().Throw<NotSupportedException>();
    }

    [TestCase(0, 0u)]
    [TestCase(6, 1u)]
    [TestCase(26, 0u)]
    [TestCase(18, 0u)]
    [TestCase(18, uint.MaxValue)]
    [TestCase(22, 0u)]
    [TestCase(22, 2147483648u)]
    [TestCase(10, 53u)]
    [TestCase(2, 54u)]
    [TestCase(34, 1u)]
    public void Read_Invalid(int offset, uint value)
    {
        var bytes = Create(24, false);
        Set(bytes, offset, value);
        AssertThat.Invoking(() => BmpFormat.Instance.Read(bytes)).Should().Throw<InvalidDataException>();
    }

    [TestCase(int.MaxValue, 1)]
    [TestCase(50000, 50000)]
    [TestCase(32768, 32768)]
    public void Read_Oversized(int width, int height)
    {
        var bytes = Create(24, false);
        Set(bytes, 18, (uint)width);
        Set(bytes, 22, (uint)height);
        AssertThat.Invoking(() => BmpFormat.Instance.Read(bytes)).Should().Throw<InvalidDataException>();
    }

    [TestCase(2)]
    [TestCase(10)]
    public void Read_OutsideFile(int offset)
    {
        var bytes = Create(24, false);
        Set(bytes, offset, uint.MaxValue);
        AssertThat.Invoking(() => BmpFormat.Instance.Read(bytes)).Should().Throw<EndOfStreamException>();
    }

    [Test]
    public void Read_InvalidPalette()
    {
        var bytes = Create(4, false);
        Set(bytes, 46, 17);
        AssertThat.Invoking(() => BmpFormat.Instance.Read(bytes)).Should().Throw<InvalidDataException>();
        Set(bytes, 46, 6);
        bytes[78] = 0xF0;
        AssertThat.Invoking(() => BmpFormat.Instance.Read(bytes)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_ZeroImageSizeAndPadding()
    {
        var bytes = Create(24, false);
        Set(bytes, 34, 0);
        bytes[63] = 255;
        BmpFormat.Instance.Read(bytes).Image.GetPixel(0, 0).Should().Equal(new Colour(0, 10, 20));
    }

    [Test]
    public async Task ReadAsync()
    {
        using var stream = new MemoryStream(Create(8, true));
        var file = await BmpFormat.Instance.ReadAsync(stream);
        file.Image.GetPixel(2, 1).Should().Equal(new Colour(5, 10, 20));
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(Create(8, true));
    }

    [Test]
    public async Task ReadAsync_Cancelled()
    {
        using var stream = new MemoryStream(Create(24, false));
        await stream.Awaiting(s => BmpFormat.Instance.ReadAsync(s, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    // Independent wire-layout fixture: three pixels per row, with distinct colour channels.
    internal static byte[] Create(int depth, bool topDown)
    {
        var offset = depth == 24 ? 54 : 78;
        var stride = depth == 24 ? 12 : 4;
        var bytes = new byte[offset + stride * 2];
        bytes[0] = (byte)'B';
        bytes[1] = (byte)'M';
        Set(bytes, 2, (uint)bytes.Length);
        Set(bytes, 10, (uint)offset);
        Set(bytes, 14, 40);
        Set(bytes, 18, 3);
        Set(bytes, 22, topDown ? unchecked((uint)-2) : 2);
        bytes[26] = 1;
        bytes[28] = (byte)depth;
        Set(bytes, 34, (uint)(stride * 2));
        if (depth != 24)
        {
            Set(bytes, 46, 6);
            for (var i = 0; i < 6; i++)
            {
                bytes[54 + i * 4] = 20;
                bytes[55 + i * 4] = 10;
                bytes[56 + i * 4] = (byte)i;
            }
        }
        for (var row = 0; row < 2; row++)
        {
            var first = (topDown ? row : 1 - row) * 3;
            for (var x = 0; x < 3; x++)
            {
                var position = offset + row * stride;
                if (depth == 24)
                {
                    bytes[position + x * 3] = 20;
                    bytes[position + x * 3 + 1] = 10;
                    bytes[position + x * 3 + 2] = (byte)(first + x);
                }
                else if (depth == 8)
                {
                    bytes[position + x] = (byte)(first + x);
                }
                else
                {
                    bytes[position + x / 2] |= (byte)((first + x) << (x % 2 == 0 ? 4 : 0));
                }
            }
        }
        return bytes;
    }

    private static void Set(byte[] bytes, int offset, uint value) => System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);

    [TestCase(4)]
    [TestCase(8)]
    public void Read_DefaultPalette(int depth)
    {
        var original = Create(depth, true);
        var count = 1 << depth;
        var offset = 54 + count * 4;
        var bytes = new byte[offset + 8];
        original.AsSpan(0, 78).CopyTo(bytes);
        original.AsSpan(78).CopyTo(bytes.AsSpan(offset));
        Set(bytes, 2, (uint)bytes.Length);
        Set(bytes, 10, (uint)offset);
        Set(bytes, 46, 0);
        bytes[57] = 255; // Reserved RGBQUAD byte is not alpha.
        var image = BmpFormat.Instance.Read(bytes).Image.Should().BeOfType<IndexedImageData>().Value;
        image.Palette.Colours.Count.Should().Equal(count);
        image.GetPixel(0, 0).Should().Equal(new Colour(0, 10, 20));
        image.Pixels.Should().SequenceEqual(0, 1, 2, 3, 4, 5);
    }

    [TestCase(4)]
    [TestCase(8)]
    [TestCase(24)]
    public void Write_RowWidths(int depth)
    {
        for (var width = 1; width <= 9; width++)
        {
            var colours = Enumerable.Range(0, width).Select(i => new Colour((byte)i, 10, 20)).ToArray();
            ImageData image = depth == 24 ? new ColourImageData(width, 1, colours)
                : new IndexedImageData(width, 1, [.. Enumerable.Range(0, width).Select(i => (byte)i)], new PaletteData(colours), depth);
            var bytes = new BmpFile(image).ToByteArray();
            var offset = depth == 24 ? 54 : 54 + width * 4;
            var stride = ((width * depth + 31) / 32) * 4;
            bytes.Length.Should().Equal(offset + stride);
            var pixelBytes = (width * depth + 7) / 8;
            bytes.AsSpan(offset + pixelBytes).ToArray().Should().SequenceEqual(new byte[stride - pixelBytes]);
            var read = BmpFormat.Instance.Read(bytes);
            for (var x = 0; x < width; x++)
            {
                read.Image.GetPixel(x, 0).Should().Equal(new Colour((byte)x, 10, 20));
            }
        }
    }

    [Test]
    public void Read_Gap()
    {
        var original = Create(24, false);
        var bytes = new byte[original.Length + 7];
        original.AsSpan(0, 54).CopyTo(bytes);
        original.AsSpan(54).CopyTo(bytes.AsSpan(61));
        Set(bytes, 2, (uint)bytes.Length);
        Set(bytes, 10, 61);
        BmpFormat.Instance.Read(bytes).Image.GetPixel(2, 1).Should().Equal(new Colour(5, 10, 20));
    }

    [Test]
    public async Task ReadAsync_NonSeekable()
    {
        await using var stream = new NonSeekableStream(Create(24, true));
        var file = await BmpFormat.Instance.ReadAsync(stream);
        file.Image.GetPixel(2, 1).Should().Equal(new Colour(5, 10, 20));
    }

    private sealed class NonSeekableStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin loc) => throw new NotSupportedException();
    }

    [TestCase(4, false)]
    [TestCase(8, true)]
    [TestCase(24, false)]
    public async Task Write_MetadataPreserved(int depth, bool topDown)
    {
        var original = Create(depth, topDown);
        var offset = depth == 24 ? 54 : 78;
        var bytes = new byte[original.Length + 8];
        original.AsSpan(0, offset).CopyTo(bytes);
        original.AsSpan(offset).CopyTo(bytes.AsSpan(offset + 3));
        bytes[offset] = 111;
        bytes[offset + 1] = 222;
        bytes[offset + 2] = 123;
        bytes[^1] = 99;
        bytes[^2] = 77;
        bytes[offset + 3 + (depth == 24 ? 11 : 3)] = 201; // Row padding.
        Set(bytes, 2, (uint)(bytes.Length - 2)); // Also retain bytes beyond bfSize.
        Set(bytes, 10, (uint)(offset + 3));
        Set(bytes, 34, 0); // Keep valid BI_RGB zero image size, not a recomputed size.
        Set(bytes, 38, 1234);
        Set(bytes, 42, 4321);
        Set(bytes, 50, 2);
        if (depth != 24)
        {
            bytes[57] = 89; // Reserved palette byte.
        }
        var file = BmpFormat.Instance.Read(bytes);
        file.InformationHeader.ImageSize.Should().Equal(0u);
        file.InformationHeader.HorizontalPixelsPerMetre.Should().Equal(1234);
        file.InformationHeader.VerticalPixelsPerMetre.Should().Equal(4321);
        file.InformationHeader.ImportantColours.Should().Equal(2u);
        file.Gap.Should().SequenceEqual(111, 222, 123);
        file.TrailingData.Should().SequenceEqual(0, 0, 0, 77, 99);
        file.Image.GetPixel(2, 1).Should().Equal(new Colour(5, 10, 20));
        file.ToByteArray().Should().SequenceEqual(bytes);
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public void Read_RgbColourTable()
    {
        var original = Create(24, true);
        var bytes = new byte[original.Length + 4];
        original.AsSpan(0, 54).CopyTo(bytes);
        original.AsSpan(54).CopyTo(bytes.AsSpan(58));
        bytes[54] = 3;
        bytes[55] = 2;
        bytes[56] = 1;
        bytes[57] = 99;
        Set(bytes, 2, (uint)bytes.Length);
        Set(bytes, 10, 58);
        Set(bytes, 46, 1);
        var file = BmpFormat.Instance.Read(bytes);
        file.Palette!.Colours.Should().SequenceEqual(new Colour(1, 2, 3));
        file.Palette.Data.Should().SequenceEqual(3, 2, 1, 99);
        file.Image.GetPixel(2, 1).Should().Equal(new Colour(5, 10, 20));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public void Read_OversizedRgbColourTable()
    {
        var bytes = Create(24, false);
        Set(bytes, 46, uint.MaxValue);
        AssertThat.Invoking(() => BmpFormat.Instance.Read(bytes)).Should().Throw<InvalidDataException>();
    }
}