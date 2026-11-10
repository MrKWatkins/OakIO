using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Resources.Aky;

namespace MrKWatkins.OakIO.Tests.Resources.Aky;

public sealed class AkyFileTests
{
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(255)]
    [TestCase(6)]
    [TestCase(9)]
    public void Constructor(int channels)
    {
        var bytes = AkyTestData.Create(channels: channels);
        var file = new AkyFile(bytes);
        file.Header.ChannelCount.Should().Equal(channels);
        file.Header.ChipCount.Should().Equal((channels + 2) / 3);
        file.Header.Version.Should().Equal(1);
        file.Header.SidProfile.Should().Equal(AkySidProfile.Cpc);
        file.Patterns.Count.Should().Equal(1);
        file.Patterns[0].Duration.Should().Equal(1);
        file.Patterns[0].Tracks.Count.Should().Equal(channels);
        file.Header.AsReadOnlySpan().ToArray().Concat(file.Body.AsReadOnlySpan().ToArray()).Should().SequenceEqual(bytes);
        bytes[^1] = 255;
        file.ToByteArray()[^1].Should().Equal((byte)0);
        file.Header.GetClock((channels + 2) / 3 - 1).Should().Equal((uint)(1773400 + (channels + 2) / 3 - 1));
    }

    [Test]
    public void Constructor_Null() => AssertThat.Invoking(() => new AkyFile(null!)).Should().Throw<ArgumentNullException>();

    [TestCase(0)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(65537)]
    public void Constructor_InvalidLength(int length) => AssertThat.Invoking(() => new AkyFile(new byte[length])).Should().Throw<InvalidDataException>();

    [TestCase(0, 0x82)]
    [TestCase(0, 0xC1)]
    [TestCase(0, 0xB1)]
    [TestCase(0, 0xE1)]
    [TestCase(1, 0)]
    [TestCase(2, 0)]
    public void Constructor_InvalidHeader(int offset, byte value)
    {
        var bytes = AkyTestData.Create();
        bytes[offset] = value;
        if (offset == 2)
        {
            bytes.AsSpan(2, 4).Clear();
        }
        AssertThat.Invoking(() => new AkyFile(bytes)).Should().Throw<InvalidDataException>();
    }

    [TestCase(8, 0)]
    [TestCase(8, 65535)]
    [TestCase(16, 7)]
    [TestCase(19, 65535)]
    public void Constructor_InvalidReference(int offset, int pointer)
    {
        var bytes = AkyTestData.Create();
        bytes.SetUInt16(offset, (ushort)pointer);
        AssertThat.Invoking(() => new AkyFile(bytes)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Constructor_TruncatedTrack()
    {
        var bytes = AkyTestData.Create();
        bytes[18] = 1;
        bytes.SetUInt16(6, 2);
        bytes.SetUInt16(21, (ushort)(bytes.Length - 2));
        AssertThat.Invoking(() => new AkyFile(bytes)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void GetLoopPatternIndex_LaterPattern()
    {
        var bytes = AkyTestData.Create();
        var data = new byte[bytes.Length + 8];
        bytes[..14].CopyTo(data, 0);
        bytes[6..14].CopyTo(data, 14);
        bytes[14..].CopyTo(data, 22);
        data.SetUInt16(24, 14);
        for (var offset = 8; offset < 22; offset += offset == 12 ? 4 : 2)
        {
            data.SetUInt16(offset, (ushort)(data.GetUInt16(offset) + 8));
        }
        for (var offset = 27; offset <= 33; offset += 3)
        {
            data.SetUInt16(offset, (ushort)(data.GetUInt16(offset) + 8));
        }
        var file = new AkyFile(data);
        file.Patterns.Count.Should().Equal(2);
        file.GetLoopPatternIndex().Should().Equal(1);
    }
}