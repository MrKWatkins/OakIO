using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Sl2;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;
using MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Screens;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Sl2;

public sealed class Sl2FormatTests
{
    [TestCase("Standard", (ScreenPaletteEncoding)0, false)]
    [TestCase("Standard", (ScreenPaletteEncoding)0, true)]
    [TestCase("Standard", (ScreenPaletteEncoding)1, false)]
    [TestCase("Standard", (ScreenPaletteEncoding)1, true)]
    [TestCase("Standard", (ScreenPaletteEncoding)2, false)]
    [TestCase("Standard", (ScreenPaletteEncoding)2, true)]
    [TestCase("Wide", (ScreenPaletteEncoding)0, false)]
    [TestCase("Wide", (ScreenPaletteEncoding)0, true)]
    [TestCase("Wide", (ScreenPaletteEncoding)1, false)]
    [TestCase("Wide", (ScreenPaletteEncoding)1, true)]
    [TestCase("Wide", (ScreenPaletteEncoding)2, false)]
    [TestCase("Wide", (ScreenPaletteEncoding)2, true)]
    [TestCase("HighResolution", (ScreenPaletteEncoding)0, false)]
    [TestCase("HighResolution", (ScreenPaletteEncoding)0, true)]
    [TestCase("HighResolution", (ScreenPaletteEncoding)1, false)]
    [TestCase("HighResolution", (ScreenPaletteEncoding)1, true)]
    [TestCase("HighResolution", (ScreenPaletteEncoding)2, false)]
    [TestCase("HighResolution", (ScreenPaletteEncoding)2, true)]
    public async Task ReadAsync_Stream(string mode, ScreenPaletteEncoding encoding, bool header)
    {
        var format = Sl2FileTests.Mode(mode);
        format.Name.Should().Equal("ZX Spectrum Next Layer 2 Screen");
        format.FileExtension.Should().Equal("sl2");
        var original = new Sl2File(ScreenTestData.Image(format.Width, format.Height, format.BitsPerPixel), format, encoding, header);
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
        var format = Sl2Format.Standard;
        var length = format.Width * format.Height;
        // The zero case is an empty file, not a valid no-palette bitmap.
        var bytes = new byte[delta == 0 ? 0 : length + delta];
        AssertThat.Invoking(() => format.Read(bytes)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_ByteArray_InvalidHeaderSize()
    {
        var format = Sl2Format.Standard;
        var file = new Sl2File(ScreenTestData.Image(format.Width, format.Height, format.BitsPerPixel), format, includeHeader: true);
        var bytes = file.ToByteArray();
        bytes[11] ^= 1;
        bytes[127] = unchecked((byte)bytes.Take(127).Sum(value => value));
        AssertThat.Invoking(() => format.Read(bytes)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_ByteArray_TruncatedHeader() =>
        AssertThat.Invoking(() => Sl2Format.Standard.Read("PLUS3DOS"u8.ToArray())).Should().Throw<InvalidDataException>();

    [Test]
    public async Task WriteAsync_ModeMismatch()
    {
        var format = Sl2Format.Standard;
        var other = Sl2Format.Wide;
        var file = new Sl2File(ScreenTestData.Image(other.Width, other.Height, other.BitsPerPixel), other);
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        ImageFormat<Sl2File> target = format;
        await target.Awaiting(f => f.WriteAsync(file, writer).AsTask()).Should().ThrowAsync<ArgumentException>();
        stream.Length.Should().Equal(0L);
    }

    [Test]
    public async Task ReadAsync_Stream_Cancelled()
    {
        using var stream = new MemoryStream(new byte[49152]);
        await Sl2Format.Standard.Awaiting(f => f.ReadAsync(stream, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task WriteAsync_Stream_Cancelled()
    {
        var format = Sl2Format.Standard;
        var file = new Sl2File(ScreenTestData.Image(format.Width, format.Height, format.BitsPerPixel), format);
        using var stream = new MemoryStream();
        await file.Awaiting(f => f.WriteAsync(stream, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [TestCase(256, 192, "Standard")]
    [TestCase(320, 256, "Wide")]
    [TestCase(640, 256, "HighResolution")]
    public void ForDimensions(int width, int height, string mode) =>
        Sl2Format.ForDimensions(width, height).Should().BeTheSameInstanceAs(Sl2FileTests.Mode(mode));

    [TestCase(255, 192)]
    [TestCase(256, 191)]
    [TestCase(128, 96)]
    [TestCase(320, 255)]
    [TestCase(640, 255)]
    public void ForDimensions_Invalid(int width, int height) =>
        AssertThat.Invoking(() => Sl2Format.ForDimensions(width, height)).Should().Throw<ArgumentException>();
}