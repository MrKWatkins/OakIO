using MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Recording.Rzx;

public sealed class RzxCreatorBlockTests
{
    [Test]
    public void Constructor()
    {
        var block = new RzxCreatorBlock("RealSpectrum", 1, 2, [0xAA, 0xBB]);

        block.Type.Should().Equal(RzxBlockType.Creator);
        block.Creator.Should().Equal("RealSpectrum");
        block.MajorVersion.Should().Equal((ushort)1);
        block.MinorVersion.Should().Equal((ushort)2);
        block.CustomData.ToArray().Should().SequenceEqual(0xAA, 0xBB);
        block.ToString().Should().Equal("Creator");
    }

    [Test]
    public void Constructor_NoCustomData()
    {
        var block = new RzxCreatorBlock("OakEmu", 3, 4);

        block.CustomData.ToArray().Should().BeEmpty();
    }

    [Test]
    public void Constructor_Bytes()
    {
        var block = new RzxCreatorBlock("OK", 1, 2, [9]);

        // Block header: ID 0x10, block length = 5 + 25 = 30.
        block.Header.Data.Should().SequenceEqual(0x10, 0x1E, 0x00, 0x00, 0x00);

        var expected = new byte[25];
        expected[0] = (byte)'O';
        expected[1] = (byte)'K';
        expected[20] = 1;
        expected[22] = 2;
        expected[24] = 9;
        block.Data.ToArray().Should().SequenceEqual(expected);
    }

    [Test]
    public void Constructor_CreatorExactly20Characters()
    {
        var creator = new string('A', 20);

        var block = new RzxCreatorBlock(creator, 1, 2);

        // The full 20 byte field is used with no null terminator.
        block.Creator.Should().Equal(creator);
        block.Data[19].Should().Equal((byte)'A');
        block.MajorVersion.Should().Equal((ushort)1);
    }

    [Test]
    public void Constructor_NullCreator_Throws()
    {
        AssertThat.Invoking(() => _ = new RzxCreatorBlock(null!, 1, 2)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_CreatorTooLong_Throws()
    {
        AssertThat.Invoking(() => _ = new RzxCreatorBlock(new string('x', 21), 1, 2)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_CreatorNotAscii_Throws()
    {
        AssertThat.Invoking(() => _ = new RzxCreatorBlock("café", 1, 2)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_ByteArrayTooShort_Throws()
    {
        var header = new RzxBlockHeader(RzxBlockType.Creator, 10);

        AssertThat.Invoking(() => _ = new RzxCreatorBlock(header, new byte[10])).Should().Throw<InvalidDataException>();
    }
}