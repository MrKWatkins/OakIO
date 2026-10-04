using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Slr;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;
using MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Screens;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Slr;

public sealed class SlrFormatTests
{
    [TestCase("EightBit", (ScreenPaletteEncoding)0, false)]
    [TestCase("EightBit", (ScreenPaletteEncoding)0, true)]
    [TestCase("EightBit", (ScreenPaletteEncoding)1, false)]
    [TestCase("EightBit", (ScreenPaletteEncoding)1, true)]
    [TestCase("EightBit", (ScreenPaletteEncoding)2, false)]
    [TestCase("EightBit", (ScreenPaletteEncoding)2, true)]
    [TestCase("FourBit", (ScreenPaletteEncoding)0, false)]
    [TestCase("FourBit", (ScreenPaletteEncoding)0, true)]
    [TestCase("FourBit", (ScreenPaletteEncoding)1, false)]
    [TestCase("FourBit", (ScreenPaletteEncoding)1, true)]
    [TestCase("FourBit", (ScreenPaletteEncoding)2, false)]
    [TestCase("FourBit", (ScreenPaletteEncoding)2, true)]
    public async Task ReadAsync_Stream(string mode, ScreenPaletteEncoding encoding, bool header)
    {
        var format = SlrFileTests.Mode(mode);
        format.Name.Should().Equal("ZX Spectrum Next Low Resolution Screen");
        format.FileExtension.Should().Equal("slr");
        var original = new SlrFile(ScreenTestData.Image(format.Width, format.Height, format.BitsPerPixel), format, encoding, header);
        using var stream = new MemoryStream(original.ToByteArray());
        var file = await format.ReadAsync(stream);
        file.ToByteArray().Should().SequenceEqual(original.ToByteArray());
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(original.ToByteArray());
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(-1)]
    [TestCase(257)]
    [TestCase(513)]
    public void Read_ByteArray_InvalidLength(int delta)
    {
        var format = SlrFormat.EightBit;
        var length = format.Width * format.Height;
        // The zero case is an empty file, not a valid no-palette bitmap.
        var bytes = new byte[delta == 0 ? 0 : length + delta];
        AssertThat.Invoking(() => format.Read(bytes)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_ByteArray_InvalidHeaderSize()
    {
        var format = SlrFormat.EightBit;
        var file = new SlrFile(ScreenTestData.Image(format.Width, format.Height, format.BitsPerPixel), format, includeHeader: true);
        var bytes = file.ToByteArray();
        bytes[11] ^= 1;
        bytes[127] = unchecked((byte)bytes.Take(127).Sum(value => value));
        AssertThat.Invoking(() => format.Read(bytes)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_ByteArray_TruncatedHeader() =>
        AssertThat.Invoking(() => SlrFormat.EightBit.Read("PLUS3DOS"u8.ToArray())).Should().Throw<InvalidDataException>();

    [Test]
    public async Task WriteAsync_ModeMismatch()
    {
        var format = SlrFormat.EightBit;
        var other = SlrFormat.FourBit;
        var file = new SlrFile(ScreenTestData.Image(other.Width, other.Height, other.BitsPerPixel), other);
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        ImageFormat<SlrFile> target = format;
        await target.Awaiting(f => f.WriteAsync(file, writer).AsTask()).Should().ThrowAsync<ArgumentException>();
        stream.Length.Should().Equal(0L);
    }

    [Test]
    public async Task ReadAsync_Stream_Cancelled()
    {
        using var stream = new MemoryStream(new byte[49152]);
        await SlrFormat.EightBit.Awaiting(f => f.ReadAsync(stream, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task WriteAsync_Stream_Cancelled()
    {
        var format = SlrFormat.EightBit;
        var file = new SlrFile(ScreenTestData.Image(format.Width, format.Height, format.BitsPerPixel), format);
        using var stream = new MemoryStream();
        await file.Awaiting(f => f.WriteAsync(stream, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [TestCase(4, "FourBit")]
    [TestCase(8, "EightBit")]
    public void ForBitsPerPixel(int bits, string mode) =>
        SlrFormat.ForBitsPerPixel(bits).Should().BeTheSameInstanceAs(SlrFileTests.Mode(mode));

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(16)]
    public void ForBitsPerPixel_Invalid(int bits) =>
        AssertThat.Invoking(() => SlrFormat.ForBitsPerPixel(bits)).Should().Throw<ArgumentOutOfRangeException>();
}