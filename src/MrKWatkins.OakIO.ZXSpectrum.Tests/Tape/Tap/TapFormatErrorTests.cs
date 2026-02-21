using MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class TapFormatErrorTests
{
    [Test]
    public void Read_ChecksumError()
    {
        using var stream = new MemoryStream();

        // Write a valid header block.
        stream.Write([0x13, 0x00]); // BlockFlagAndChecksumLength = 19.
        stream.WriteByte(0x00); // Flag = Header.
        stream.WriteByte(0x03); // Type = Code.
        stream.Write("test      "u8); // Filename 10 bytes.
        stream.Write([0x02, 0x00]); // DataBlockLength = 2.
        stream.Write([0x00, 0x80]); // Parameter1 = 0x8000.
        stream.Write([0x00, 0x80]); // Parameter2 = 32768.
        stream.WriteByte(0xFF); // Wrong checksum.

        stream.Position = 0;

        AssertThat.Invoking(() => TapFormat.Instance.Read(stream))
            .Should().Throw<InvalidOperationException>()
            .Exception.Message.Should().Match("Expected TAP block to have checksum*");
    }

    [Test]
    public void Read_UnknownBlockType()
    {
        using var stream = new MemoryStream();

        // Write a block with unknown flag type (not 0x00 or 0xFF).
        // blockFlagAndChecksumLength = 3: flag (1) + checksum (1) + data (blockFlagAndChecksumLength - 2 = 1).
        stream.Write([0x03, 0x00]); // BlockFlagAndChecksumLength = 3.
        stream.WriteByte(0x42); // Flag = Unknown (not Header or Data).
        // No extra data bytes (data.Length = 3 - 2 = 1, but flag is separate, so data.Length = 1).
        stream.WriteByte(0xAA); // Data byte.

        // Checksum = flag XOR data = 0x42 XOR 0xAA = 0xE8.
        stream.WriteByte(0xE8);

        stream.Position = 0;

        AssertThat.Invoking(() => TapFormat.Instance.Read(stream))
            .Should().Throw<InvalidOperationException>()
            .Exception.Message.Should().Equal("Unexpected TAP block type 0x42.");
    }

    [Test]
    public void Read_EmptyStream()
    {
        using var stream = new MemoryStream();

        AssertThat.Invoking(() => TapFormat.Instance.Read(stream))
            .Should().ThrowArgumentException("Value was empty.", "stream");
    }

    [Test]
    public void Read_ByteArray()
    {
        // Write a simple valid TAP file as bytes.
        var file = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var bytes = TapFormat.Instance.Write(file);

        var result = TapFormat.Instance.Read(bytes);
        result.Blocks.Should().HaveCount(2);
    }

    [Test]
    public void Write_ThrowsForWrongFileType()
    {
        var pzxData = BuildMinimalPzxData();
        using var pzxStream = new MemoryStream(pzxData);
        var pzxFile = PzxFormat.Instance.Read(pzxStream);

        using var output = new MemoryStream();
        AssertThat.Invoking(() => TapFormat.Instance.Write(pzxFile, output))
            .Should().ThrowArgumentException("Value is not of type TapFile.", "file");
    }

    private static byte[] BuildMinimalPzxData()
    {
        using var stream = new MemoryStream();
        stream.Write("PZXT"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);
        return stream.ToArray();
    }
}