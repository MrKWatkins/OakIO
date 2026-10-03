using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxp;

public sealed class NxpColourEntryTests
{
    [Test]
    public void Constructor_ByteArray()
    {
        var entry = new NxpColourEntry(new byte[] { 0b101_011_10, 1 });
        entry.Red.Should().Equal((byte)5);
        entry.Green.Should().Equal((byte)3);
        entry.Blue.Should().Equal((byte)5);
        entry.Colour.Should().Equal(new Colour(182, 109, 182));
        entry.Data.Should().SequenceEqual(0b101_011_10, 1);
        entry.Length.Should().Equal(2);
    }

    [Test]
    public void Constructor_Colour()
    {
        var entry = new NxpColourEntry(new Colour(255, 128, 36));
        entry.Data.Should().SequenceEqual(0xF0, 1);
        entry.Red.Should().Equal((byte)7);
        entry.Green.Should().Equal((byte)4);
        entry.Blue.Should().Equal((byte)1);
        entry.Colour.Should().Equal(new Colour(255, 146, 36));
    }

    [Test]
    public void Constructor_Colour_AllLevels()
    {
        byte[] expanded = [0, 36, 73, 109, 146, 182, 219, 255];
        for (var value = 0; value < 512; value++)
        {
            byte[] bytes = [(byte)(value >> 1), (byte)(value & 1)];
            var entry = new NxpColourEntry(bytes);
            var red = value >> 6;
            var green = (value >> 3) & 7;
            var blue = value & 7;
            entry.Red.Should().Equal((byte)red);
            entry.Green.Should().Equal((byte)green);
            entry.Blue.Should().Equal((byte)blue);
            entry.Colour.Should().Equal(new Colour(expanded[red], expanded[green], expanded[blue]));
            new NxpColourEntry(entry.Colour).Data.Should().SequenceEqual(bytes);
        }
    }

    [Test]
    public void Constructor_Colour_AllComponentValues()
    {
        int[] thresholds = [19, 55, 92, 128, 164, 201, 237];
        for (var value = 0; value <= 255; value++)
        {
            var expected = (byte)thresholds.Count(threshold => value >= threshold);
            var red = new NxpColourEntry(new Colour((byte)value, 0, 0));
            red.Red.Should().Equal(expected);
            red.Green.Should().Equal((byte)0);
            red.Blue.Should().Equal((byte)0);
            var green = new NxpColourEntry(new Colour(0, (byte)value, 0));
            green.Red.Should().Equal((byte)0);
            green.Green.Should().Equal(expected);
            green.Blue.Should().Equal((byte)0);
            var blue = new NxpColourEntry(new Colour(0, 0, (byte)value));
            blue.Red.Should().Equal((byte)0);
            blue.Green.Should().Equal((byte)0);
            blue.Blue.Should().Equal(expected);
        }
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(3)]
    public void Constructor_ByteArray_InvalidLength(int length) =>
        AssertThat.Invoking(() => new NxpColourEntry(new byte[length])).Should().Throw<InvalidDataException>();

    [TestCase(2)]
    [TestCase(128)]
    [TestCase(255)]
    public void Constructor_ByteArray_InvalidBlueBit(byte value) =>
        AssertThat.Invoking(() => new NxpColourEntry(new byte[] { 0, value })).Should().Throw<InvalidDataException>();

    [TestCase(0)]
    [TestCase(128)]
    [TestCase(254)]
    public void Constructor_Colour_Alpha(byte alpha) =>
        AssertThat.Invoking(() => new NxpColourEntry(new Colour(1, 2, 3, alpha))).Should().Throw<ArgumentException>();
}