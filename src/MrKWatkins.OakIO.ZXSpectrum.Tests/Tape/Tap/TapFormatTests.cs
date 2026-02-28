using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

public sealed class TapFormatTests : TapTestFixture
{
    [Test]
    public void Read()
    {
        using var z80Test = OpenZ80Test();

        var file = TapFormat.Instance.Read(z80Test);
        file.Format.Should().BeTheSameInstanceAs(TapFormat.Instance);
        file.Blocks.Should().HaveCount(4);

        var basicHeader = file.Blocks[0].Should().BeOfType<HeaderBlock>().Value;

        basicHeader.HeaderType.Should().Equal(TapHeaderType.Program);
        basicHeader.Filename.Should().Equal("z80full");
        basicHeader.DataBlockLength.Should().Equal(42);
        basicHeader.Length.Should().Equal(17);
        basicHeader.Location.Should().Equal(23755);
        basicHeader.Parameter1.Should().Equal(10);
        basicHeader.Parameter2.Should().Equal(42);
        basicHeader.Checksum.Should().Equal(75);
        basicHeader.Header.BlockLength.Should().Equal(17);
        basicHeader.Header.BlockFlagAndChecksumLength.Should().Equal(19);
        basicHeader.Trailer.Checksum.Should().Equal(75);

        var basicData = file.Blocks[1].Should().BeOfType<DataBlock>().Value;
        basicData.Length.Should().Equal(42);
        basicData.Checksum.Should().Equal(139);
        basicData.Header.BlockLength.Should().Equal(42);
        basicData.Header.BlockFlagAndChecksumLength.Should().Equal(44);
        basicData.Trailer.Checksum.Should().Equal(139);

        var codeHeader = file.Blocks[2].Should().BeOfType<HeaderBlock>().Value;

        codeHeader.HeaderType.Should().Equal(TapHeaderType.Code);
        codeHeader.Filename.Should().Equal("z80full");
        codeHeader.DataBlockLength.Should().Equal(14298);
        codeHeader.Length.Should().Equal(17);
        codeHeader.Location.Should().Equal(32768);
        codeHeader.Parameter1.Should().Equal(32768);
        codeHeader.Parameter2.Should().Equal(0);
        codeHeader.Checksum.Should().Equal(47);
        codeHeader.Header.BlockLength.Should().Equal(17);
        codeHeader.Header.BlockFlagAndChecksumLength.Should().Equal(19);
        codeHeader.Trailer.Checksum.Should().Equal(47);

        var codeData = file.Blocks[3].Should().BeOfType<DataBlock>().Value;
        codeData.Length.Should().Equal(14298);
        codeData.Checksum.Should().Equal(208);
        codeData.Header.BlockLength.Should().Equal(14298);
        codeData.Header.BlockFlagAndChecksumLength.Should().Equal(14300);
        codeData.Trailer.Checksum.Should().Equal(208);
    }

    [Test]
    public void RoundTrip()
    {
        using var z80Test = OpenZ80Test();

        var tap = TapFormat.Instance.Read(z80Test);

        var actual = TapFormat.Instance.Write(tap);

        z80Test.Seek(0, SeekOrigin.Begin);
        var expected = z80Test.ReadAllBytes();

        actual.Should().SequenceEqual(expected);
    }

    [Test]
    public void Write()
    {
        // Example from https://sinclair.wiki.zxnet.co.uk/wiki/TAP_format.
        var rom = CreateRom();
        var file = TapFile.CreateCode("ROM", 0, rom);

        using var output = new MemoryStream();

        TapFormat.Instance.Write(file, output);

        var expected = new byte[]
        {
            // Header.
            0x13, 0x00, // Length.
            0x00, // Flag = Header.
            0x03, // Type = Code.
            0x52, 0x4F, 0x4D, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, // Filename = "ROM".
            0x02, 0x00, // Length of the data block = 2.
            0x00, 0x00, // Location = 0.
            0x00, 0x80, // 32768.
            0xF1, // Checksum.

            // Block.
            0x04, 0x00, // Length = 4. (2 + flag + checksum)
            0xFF, // Flag = Data.
            0xF3, 0xAF, // Data.
            0xA3 // Checksum.
        };

        var actual = output.ToArray();
        actual.Should().SequenceEqual(expected);
    }

    [Test]
    public void ConvertToWav()
    {
        using var z80Test = OpenZ80Test();

        var tap = TapFormat.Instance.Read(z80Test);

        var wav = IOFileConversion.ConvertToWav(tap, 22050);

        wav.SampleRate.Should().Equal(22050u);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Pure]
    private static byte[] CreateRom() => [0xF3, 0xAF];
}