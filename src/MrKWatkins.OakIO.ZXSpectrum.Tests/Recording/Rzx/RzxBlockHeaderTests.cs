using MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Recording.Rzx;

public sealed class RzxBlockHeaderTests
{
    [Test]
    public void Constructor()
    {
        var header = new RzxBlockHeader(RzxBlockType.Creator, 24);

        header.Type.Should().Equal(RzxBlockType.Creator);
        header.BlockLength.Should().Equal(29U);
        header.DataLength.Should().Equal(24U);
    }

    [Test]
    public void Constructor_ByteArray()
    {
        var header = new RzxBlockHeader(new RzxBlockHeader(RzxBlockType.Snapshot, 12).Data.ToArray());

        header.Type.Should().Equal(RzxBlockType.Snapshot);
        header.BlockLength.Should().Equal(17U);
        header.DataLength.Should().Equal(12U);
    }

    [Test]
    public void Constructor_BlockLengthTooSmall_Throws()
    {
        var data = new byte[5];
        data[0] = (byte)RzxBlockType.Creator;
        // Block length of 4 is smaller than the five byte header.
        data[1] = 4;

        AssertThat.Invoking(() => _ = new RzxBlockHeader(data)).Should().Throw<InvalidDataException>();
    }

    [TestCase(RzxBlockType.Creator, "Creator")]
    [TestCase(RzxBlockType.Snapshot, "Snapshot")]
    [TestCase(RzxBlockType.InputRecording, "InputRecording")]
    public void ToStringTest(RzxBlockType type, string expected)
    {
        new RzxBlockHeader(type, 13).ToString().Should().Equal(expected);
    }
}