using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxb;

public sealed class TileBlocksToNxbConverterTests
{
    [Test]
    public void Constructor()
    {
        var source = new NxbFormat(3, 2, 8);
        var target = new NxbFormat(3, 2, 16);
        var converter = new TileBlocksToNxbConverter(source, target);
        converter.SourceFormat.Should().BeTheSameInstanceAs(source);
        converter.TargetFormat.Should().BeTheSameInstanceAs(target);
    }
    [Test]
    public void Constructor_NullSource() => AssertThat.Invoking(() => new TileBlocksToNxbConverter(null!, new NxbFormat(1, 1, 8))).Should().Throw<ArgumentNullException>();
    [Test]
    public void Constructor_NullTarget() => AssertThat.Invoking(() => new TileBlocksToNxbConverter(new NxbFormat(1, 1, 8), null!)).Should().Throw<ArgumentNullException>();

    [TestCaseSource(typeof(NxbTestData), nameof(NxbTestData.Collections))]
    public void Convert_TileBlocksFile(int bits, int count)
    {
        var source = new NxbFormat(3, 2, bits).Read(NxbTestData.Bytes(bits, count));
        var target = new NxbFormat(3, 2, bits);
        var file = new TileBlocksToNxbConverter(source.Format, target).Convert(source);
        file.Should().NotBeTheSameInstanceAs(source);
        file.Format.Should().BeTheSameInstanceAs(target);
        file.ToByteArray().Should().SequenceEqual(NxbTestData.Bytes(bits, count));
        for (var index = 0; index < count; index++)
        {
            file.Entries[index].Should().NotBeTheSameInstanceAs(source.Entries[index]);
        }
    }

    [TestCase(8, 16)]
    [TestCase(16, 8)]
    public void Convert_TileBlocksFile_IndexWidth(int sourceBits, int targetBits)
    {
        var entries = NxbTestData.Entries(8, 2);
        var source = new NxbFile(new TileBlocksData(3, 2, entries), new NxbFormat(3, 2, sourceBits));
        var file = new TileBlocksToNxbConverter(source.Format, new NxbFormat(3, 2, targetBits)).Convert(source);
        var expected = targetBits == 8 ? NxbTestData.Bytes(8, 2) : [0, 0, 1, 0, 255, 0, 0, 0, 205, 0, 255, 0, 255, 0, 205, 0, 0, 0, 255, 0, 1, 0, 0, 0];
        file.ToByteArray().Should().SequenceEqual(expected);
        file.Blocks.Entries.Should().SequenceEqual(entries);
    }

    [TestCase(256)]
    [TestCase(257)]
    public void Convert_TileBlocksFile_IndexOverflow(int index)
    {
        var source = new NxbFile(new TileBlocksData(1, 1, [new(index)]), new NxbFormat(1, 1, 16));
        AssertThat.Invoking(() => new TileBlocksToNxbConverter(source.Format, new NxbFormat(1, 1, 8)).Convert(source)).Should().Throw<ArgumentException>();
        source.Blocks.Entries[0].TileIndex.Should().Equal(index);
    }

    [Test]
    public void Convert_TileBlocksFile_Attributes()
    {
        TileMapEntry[] entries = [new(0, 1), new(0) { MirrorX = true }, new(0) { MirrorY = true }, new(0) { Rotate = true }, new(0) { Priority = true }];
        foreach (var entry in entries)
        {
            var source = new AttributedFile(new TileBlocksData(1, 1, [entry]));
            AssertThat.Invoking(() => new TileBlocksToNxbConverter(source.Format, new NxbFormat(1, 1, 8)).Convert(source)).Should().Throw<ArgumentException>();
            source.Blocks.Entries.Should().SequenceEqual(entry);
        }
    }

    [Test]
    public void Convert_TileBlocksFile_InvalidDimensions()
    {
        var source = new NxbFormat(3, 2, 8).Read([]);
        AssertThat.Invoking(() => new TileBlocksToNxbConverter(source.Format, new NxbFormat(2, 3, 8)).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Convert_TileBlocksFile_Null() =>
        AssertThat.Invoking(() => new TileBlocksToNxbConverter(new NxbFormat(1, 1, 8), new NxbFormat(1, 1, 8)).Convert(null!)).Should().Throw<ArgumentNullException>();
    [Test]
    public void Convert_IOFile()
    {
        var format = new NxbFormat(1, 1, 8);
        IOFileConverter converter = new TileBlocksToNxbConverter(format, format);
        converter.Convert(format.Read([42])).ToByteArray().Should().SequenceEqual(42);
    }

    private sealed class AttributedFile(TileBlocksData blocks) : TileBlocksFile(AttributedFormat.Instance)
    {
        public override TileBlocksData Blocks => blocks;
    }
    private sealed class AttributedFormat() : TileBlocksFormat<AttributedFile>("Attributed", "attributed")
    {
        internal static readonly AttributedFormat Instance = new();
        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => throw new NotSupportedException();
        protected override ValueTask WriteAsync(AttributedFile file, IBinaryWriter writer) => throw new NotSupportedException();
    }
}