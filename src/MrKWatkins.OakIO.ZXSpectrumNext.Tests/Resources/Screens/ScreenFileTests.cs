using MrKWatkins.OakIO.ZXSpectrum.Plus3Dos;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Sl2;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Screens;

public sealed class ScreenFileTests
{
    [Test]
    public void Constructor_ByteArray_RawSignature()
    {
        var bytes = new byte[49152];
        "PLUS3DOS"u8.CopyTo(bytes);
        var file = Sl2Format.Standard.Read(bytes);
        file.Header.Should().BeNull();
        file.PixelData.Data.Take(8).Should().SequenceEqual("PLUS3DOS"u8.ToArray());
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public void Constructor_ByteArray_HeaderMetadata()
    {
        var header = Plus3DosHeader.Create(49152).Data.ToArray();
        header[9] = 2;
        header[10] = 3;
        header[126] = 42;
        header[127] = unchecked((byte)header.Take(127).Sum(value => value));
        var bytes = header.Concat(new byte[49152]).ToArray();
        var file = Sl2Format.Standard.Read(bytes);
        file.Header!.Issue.Should().Equal((byte)2);
        file.Header.Version.Should().Equal((byte)3);
        file.Header.Data[126].Should().Equal((byte)42);
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public void Constructor_IndexedImageData_Components()
    {
        ScreenFile file = new Sl2File(ScreenTestData.Image(256, 192, 8), Sl2Format.Standard, ScreenPaletteEncoding.None, true);
        file.Header!.FileSize.Should().Equal((uint)49280);
        file.PixelData.Length.Should().Equal(49152);
        file.Palette.Length.Should().Equal(0);
        file.Image.Width.Should().Equal(256);
        file.Image.Height.Should().Equal(192);
    }

    [Test]
    public void Constructor_ByteArray_TruncatedBitmapWithHeader()
    {
        var bytes = Plus3DosHeader.Create(1).Data.Concat(new byte[1]).ToArray();
        AssertThat.Invoking(() => Sl2Format.Standard.Read(bytes)).Should().Throw<InvalidDataException>();
    }
}