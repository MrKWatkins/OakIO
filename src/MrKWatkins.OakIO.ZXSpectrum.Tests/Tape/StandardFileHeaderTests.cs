using MrKWatkins.OakIO.ZXSpectrum.Tape;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;
using PzxDataBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx.DataBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape;

public sealed class StandardFileHeaderTests
{
    [Test]
    public void TryCreate()
    {
        var data = CreateValidTapeHeaderBlock();

        StandardFileHeader.TryCreate(data, out var header).Should().BeTrue();
        header!.Type.Should().Equal(TapHeaderType.Code);
        header.Filename.Should().Equal("Test");
        header.DataBlockLength.Should().Equal(100);
        header.Parameter1.Should().Equal(0x8000);
        header.Parameter2.Should().Equal(0x8000);
    }

    [Test]
    public void TryCreate_Program()
    {
        var data = CreateValidTapeHeaderBlock(TapHeaderType.Program, "Loader", 200, 10, 200);

        StandardFileHeader.TryCreate(data, out var header).Should().BeTrue();
        header!.Type.Should().Equal(TapHeaderType.Program);
        header.Filename.Should().Equal("Loader");
        header.DataBlockLength.Should().Equal(200);
        header.Parameter1.Should().Equal(10);
        header.Parameter2.Should().Equal(200);
    }

    [Test]
    public void TryCreate_WrongLength()
    {
        StandardFileHeader.TryCreate(new byte[18], out var header).Should().BeFalse();
        header.Should().BeNull();
    }

    [Test]
    public void TryCreate_WrongFlagByte()
    {
        var data = CreateValidTapeHeaderBlock();
        data[0] = 0xFF; // Data flag, not header.

        // Recalculate checksum.
        byte checksum = 0;
        for (var i = 0; i < 18; i++)
        {
            checksum ^= data[i];
        }

        data[18] = checksum;

        StandardFileHeader.TryCreate(data, out var header).Should().BeFalse();
        header.Should().BeNull();
    }

    [Test]
    public void TryCreate_InvalidHeaderType()
    {
        var data = CreateValidTapeHeaderBlock();
        data[1] = 4; // Invalid type.

        // Recalculate checksum.
        byte checksum = 0;
        for (var i = 0; i < 18; i++)
        {
            checksum ^= data[i];
        }

        data[18] = checksum;

        StandardFileHeader.TryCreate(data, out var header).Should().BeFalse();
        header.Should().BeNull();
    }

    [Test]
    public void TryCreate_BadChecksum()
    {
        var data = CreateValidTapeHeaderBlock();
        data[18] ^= 0xFF; // Corrupt checksum.

        StandardFileHeader.TryCreate(data, out var header).Should().BeFalse();
        header.Should().BeNull();
    }

    [Test]
    public void TryGetStandardFileHeader_TapHeaderBlock()
    {
        var block = HeaderBlock.CreateCode("game", 0x8000, 4096);

        block.TryGetStandardFileHeader(out var header).Should().BeTrue();
        header!.Type.Should().Equal(TapHeaderType.Code);
        header.Filename.Should().Equal("game");
        header.DataBlockLength.Should().Equal(4096);
        header.Parameter1.Should().Equal(0x8000);
        header.Parameter2.Should().Equal(32768);
    }

    [Test]
    public void TryGetStandardFileHeader_TzxStandardSpeedDataBlock()
    {
        var tapeData = CreateValidTapeHeaderBlock(TapHeaderType.Code, "MyCode", 500, 0x6000);
        var tzxHeaderData = new byte[] { 0xE8, 0x03, 0x13, 0x00 }; // Pause 1000ms, length 19.

        var block = new StandardSpeedDataBlock(tzxHeaderData, tapeData);

        block.TryGetStandardFileHeader(out var header).Should().BeTrue();
        header!.Type.Should().Equal(TapHeaderType.Code);
        header.Filename.Should().Equal("MyCode");
        header.DataBlockLength.Should().Equal(500);
        header.Parameter1.Should().Equal(0x6000);
        header.Parameter2.Should().Equal(0x8000);
    }

    [Test]
    public void TryGetStandardFileHeader_TzxStandardSpeedDataBlock_NotAHeader()
    {
        var tapeData = new byte[100]; // Wrong length for a standard file header.
        tapeData[0] = 0xFF; // Data flag.
        var tzxHeaderData = new byte[] { 0xE8, 0x03, 0x64, 0x00 }; // Pause 1000ms, length 100.

        var block = new StandardSpeedDataBlock(tzxHeaderData, tapeData);

        block.TryGetStandardFileHeader(out var header).Should().BeFalse();
        header.Should().BeNull();
    }

    [Test]
    public void TryGetStandardFileHeader_PzxDataBlock()
    {
        var tapeData = CreateValidTapeHeaderBlock(TapHeaderType.Program, "Basic", 300, 10, 300);

        // PZX DataHeader: 12 bytes.
        // [0-3]: SizeOfBlockExcludingTagAndSizeField = 8 (header extras) + 19 (body) = 27.
        // [4-7]: SizeInBits = 19 * 8 = 152.
        // [8-9]: Tail = 0.
        // [10]: NumberOfPulseInZeroBitSequence = 0.
        // [11]: NumberOfPulseInOneBitSequence = 0.
        var pzxHeaderData = new byte[] { 0x1B, 0x00, 0x00, 0x00, 0x98, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        var block = new PzxDataBlock(pzxHeaderData, tapeData);

        block.TryGetStandardFileHeader(out var header).Should().BeTrue();
        header!.Type.Should().Equal(TapHeaderType.Program);
        header.Filename.Should().Equal("Basic");
        header.DataBlockLength.Should().Equal(300);
        header.Parameter1.Should().Equal(10);
        header.Parameter2.Should().Equal(300);
    }

    [Test]
    public void TryGetStandardFileHeader_PzxDataBlock_NotAHeader()
    {
        var tapeData = new byte[100]; // Wrong length for a standard file header.
        tapeData[0] = 0xFF; // Data flag.

        // PZX DataHeader for 100-byte body.
        var pzxHeaderData = new byte[] { 0x6C, 0x00, 0x00, 0x00, 0x20, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        var block = new PzxDataBlock(pzxHeaderData, tapeData);

        block.TryGetStandardFileHeader(out var header).Should().BeFalse();
        header.Should().BeNull();
    }

    [Pure]
    private static byte[] CreateValidTapeHeaderBlock(
        TapHeaderType type = TapHeaderType.Code,
        string filename = "Test",
        ushort dataBlockLength = 100,
        ushort parameter1 = 0x8000,
        ushort parameter2 = 0x8000)
    {
        var data = new byte[19];
        data[0] = 0x00; // Flag byte (header).
        data[1] = (byte)type;

        // Filename: 10 bytes, space-padded.
        var paddedFilename = filename.PadRight(10);
        for (var i = 0; i < 10; i++)
        {
            data[2 + i] = (byte)paddedFilename[i];
        }

        data[12] = (byte)(dataBlockLength & 0xFF);
        data[13] = (byte)(dataBlockLength >> 8);
        data[14] = (byte)(parameter1 & 0xFF);
        data[15] = (byte)(parameter1 >> 8);
        data[16] = (byte)(parameter2 & 0xFF);
        data[17] = (byte)(parameter2 >> 8);

        // Checksum: XOR of bytes 0-17.
        byte checksum = 0;
        for (var i = 0; i < 18; i++)
        {
            checksum ^= data[i];
        }

        data[18] = checksum;

        return data;
    }
}