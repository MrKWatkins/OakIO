using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

public sealed class HeaderBlockTests
{
    [Test]
    public void CreateCode()
    {
        var block = HeaderBlock.CreateCode("test", 0x8000, 100);
        block.HeaderType.Should().Equal(TapHeaderType.Code);
        block.Filename.Should().Equal("test");
        block.DataBlockLength.Should().Equal(100);
        block.Parameter1.Should().Equal(0x8000);
        block.Parameter2.Should().Equal(32768);
        block.Location.Should().Equal(0x8000);
    }

    [Test]
    public void CreateProgram()
    {
        var block = HeaderBlock.CreateProgram("prog", 42, 10, 42);
        block.HeaderType.Should().Equal(TapHeaderType.Program);
        block.Filename.Should().Equal("prog");
        block.DataBlockLength.Should().Equal(42);
        block.Parameter1.Should().Equal(10);
        block.Parameter2.Should().Equal(42);
        block.Location.Should().Equal(23755);
    }

    [Test]
    public void CreateProgram_DefaultAutostartAndVariableArea()
    {
        var block = HeaderBlock.CreateProgram("prog", 42);
        block.HeaderType.Should().Equal(TapHeaderType.Program);
        block.Filename.Should().Equal("prog");
        block.DataBlockLength.Should().Equal(42);
        block.Parameter1.Should().Equal(65535);
        block.Parameter2.Should().Equal(42);
    }

    [Test]
    public void Location_ThrowsForUnsupportedType()
    {
        // Modify the type to NumberArray. Since we can't modify directly, we create one
        // through the format reader with a custom stream.
        using var stream = new MemoryStream();

        // Write a header block with NumberArray type.
        var flagAndLength = (ushort)19;
        stream.WriteByte((byte)flagAndLength);
        stream.WriteByte((byte)(flagAndLength >> 8));
        stream.WriteByte((byte)TapBlockType.Header); // Flag.
        stream.WriteByte((byte)TapHeaderType.NumberArray); // Type.
        stream.Write("test      "u8); // Filename 10 bytes.
        stream.Write([0x02, 0x00]); // DataBlockLength = 2.
        stream.Write([0x00, 0x00]); // Parameter1.
        stream.Write([0x00, 0x00]); // Parameter2.

        // Calculate checksum.
        stream.Position = 2; // Skip length word.
        byte checksum = 0;
        for (var i = 2; i < stream.Length; i++)
        {
            checksum ^= (byte)stream.ReadByte();
        }
        stream.WriteByte(checksum);

        stream.Position = 0;
        var tapFile = TapFormat.Instance.Read(stream);
        var block = tapFile.Blocks[0].Should().BeOfType<HeaderBlock>().Value;
        block.HeaderType.Should().Equal(TapHeaderType.NumberArray);
        block.Invoking(b => _ = b.Location).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void ToString_Program()
    {
        var block = HeaderBlock.CreateProgram("test", 42);
        block.ToString().Should().Equal("Program: test");
    }

    [Test]
    public void ToString_Code()
    {
        var block = HeaderBlock.CreateCode("test", 0x8000, 100);
        block.ToString().Should().Equal("Bytes: test");
    }

    [Test]
    public void ToString_NumberArray()
    {
        using var stream = new MemoryStream();
        var flagAndLength = (ushort)19;
        stream.WriteByte((byte)flagAndLength);
        stream.WriteByte((byte)(flagAndLength >> 8));
        stream.WriteByte((byte)TapBlockType.Header);
        stream.WriteByte((byte)TapHeaderType.NumberArray);
        stream.Write("numbers   "u8);
        stream.Write([0x02, 0x00, 0x00, 0x00, 0x00, 0x00]);

        stream.Position = 2;
        byte checksum = 0;
        for (var i = 2; i < stream.Length; i++)
        {
            checksum ^= (byte)stream.ReadByte();
        }
        stream.WriteByte(checksum);

        stream.Position = 0;
        var tapFile = TapFormat.Instance.Read(stream);
        var block = tapFile.Blocks[0].Should().BeOfType<HeaderBlock>().Value;
        block.ToString().Should().Equal("Number array: numbers");
    }

    [Test]
    public void ToString_CharacterArray()
    {
        using var stream = new MemoryStream();
        var flagAndLength = (ushort)19;
        stream.WriteByte((byte)flagAndLength);
        stream.WriteByte((byte)(flagAndLength >> 8));
        stream.WriteByte((byte)TapBlockType.Header);
        stream.WriteByte((byte)TapHeaderType.CharacterArray);
        stream.Write("chars     "u8);
        stream.Write([0x02, 0x00, 0x00, 0x00, 0x00, 0x00]);

        stream.Position = 2;
        byte checksum = 0;
        for (var i = 2; i < stream.Length; i++)
        {
            checksum ^= (byte)stream.ReadByte();
        }
        stream.WriteByte(checksum);

        stream.Position = 0;
        var tapFile = TapFormat.Instance.Read(stream);
        var block = tapFile.Blocks[0].Should().BeOfType<HeaderBlock>().Value;
        block.ToString().Should().Equal("Character array: chars");
    }
}