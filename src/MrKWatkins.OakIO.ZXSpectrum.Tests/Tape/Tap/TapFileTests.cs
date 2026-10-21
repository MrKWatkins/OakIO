using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;
using PzxFile = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx.PzxFile;
using PzxFormat = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx.PzxFormat;
using PzxDataBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx.DataBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

public sealed class TapFileTests : TapTestFixture
{
    [Test]
    public void CreateTapBas_Blocks()
    {
        byte[] code = [0xF3, 0xAF];
        byte[] forest = [12, 34, 56];
        var inputs = new List<TapBasBlock>
        {
            new(0x8000, code),
            new(0xC000, forest)
            {
                Bank = 0
            },
            new(0xD000, new byte[]
            {
                78, 90
            })
            {
                Bank = 1
            }
        };
        var file = TapFile.CreateTapBas("game", inputs, new TapBasOptions
        {
            EntryPoint = 0x8000,
            EntryBank = 0
        });
        code.AsSpan().Clear();
        forest.AsSpan().Clear();
        inputs.Clear();
        file.Blocks.Count.Should().Equal(8);
        var loader = file.Blocks[0].Should().BeOfType<HeaderBlock>().Value;
        loader.HeaderType.Should().Equal(TapHeaderType.Program);
        loader.Filename.Should().Equal("loader");
        loader.Parameter1.Should().Equal((ushort)10);
        loader.DataBlockLength.Should().Equal(file.Blocks[1].Length);
        int[] locations = [0x8000, 0xC000, 0xD000];
        byte[][] expected = [[0xF3, 0xAF], [12, 34, 56], [78, 90]];
        for (var index = 0; index < expected.Length; index++)
        {
            var header = file.Blocks[2 + index * 2].Should().BeOfType<HeaderBlock>().Value;
            header.HeaderType.Should().Equal(TapHeaderType.Code);
            header.Filename.Should().Equal("game");
            header.Location.Should().Equal((ushort)locations[index]);
            header.DataBlockLength.Should().Equal((ushort)expected[index].Length);
            file.Blocks[3 + index * 2].Data.Should().SequenceEqual(expected[index]);
        }
        var bytes = file.ToByteArray();
        var read = TapFormat.Instance.Read(bytes);
        read.ToByteArray().Should().SequenceEqual(bytes);
        read.Blocks.Count.Should().Equal(8);
        foreach (var block in read.Blocks)
        {
            var checksum = (byte)block.Header.Type;
            foreach (var value in block.Data)
            {
                checksum ^= value;
            }
            block.Trailer.Checksum.Should().Equal(checksum);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void CreateTapBas_BlocksMatchesLegacy(bool entry)
    {
        byte[] code = [0xF3, 0xAF];
        var legacy = TapFile.CreateTapBas("game", 0x8000, code, entry ? 0x8000 : null);
        var current = TapFile.CreateTapBas("game", [new TapBasBlock(0x8000, code)], new TapBasOptions
        {
            EntryPoint = entry ? 0x8000 : null
        });
        current.ToByteArray().Should().SequenceEqual(legacy.ToByteArray());
    }

    [Test]
    public void CreateTapBas_BlocksDefaultOptions()
    {
        var file = TapFile.CreateTapBas("game", [new TapBasBlock(0x8000, new byte[1])]);
        file.ToByteArray().Should().SequenceEqual(TapFile.CreateTapBas("game", 0x8000, [0]).ToByteArray());
    }

    [Test]
    public void CreateTapBas_BlocksConversions()
    {
        var tap = TapFile.CreateTapBas("game", [new TapBasBlock(0x8000, new byte[]
        {
            0xF3, 0xAF
        }), new TapBasBlock(0xC000, new byte[]
        {
            12, 34
        })
        {
            Bank = 3
        }]);
        var tzx = IOFileConversion.Convert<TzxFile>(tap);
        var pzx = IOFileConversion.Convert<PzxFile>(tap);
        tzx = TzxFormat.Instance.Read(tzx.ToByteArray());
        pzx = PzxFormat.Instance.Read(pzx.ToByteArray());
        var tzxData = tzx.Blocks.OfType<StandardSpeedDataBlock>().ToArray();
        var pzxData = pzx.Blocks.OfType<PzxDataBlock>().ToArray();
        tzxData.Length.Should().Equal(tap.Blocks.Count);
        pzxData.Length.Should().Equal(tap.Blocks.Count);
        for (var index = 0; index < tap.Blocks.Count; index++)
        {
            var block = tap.Blocks[index];
            var expected = new byte[block.Length + 2];
            expected[0] = (byte)block.Header.Type;
            block.CopyTo(expected.AsSpan(1));
            expected[^1] = block.Trailer.Checksum;
            tzxData[index].Data.Should().SequenceEqual(expected);
            pzxData[index].DataStream.ToArray().Should().SequenceEqual(expected);
        }
    }

    [Test]
    public void CreateTapBas_BlocksNullFilename() =>
        AssertThat.Invoking(() => TapFile.CreateTapBas(null!, [new TapBasBlock(0x8000, new byte[1])])).Should().Throw<ArgumentNullException>();

    [Test]
    public void CreateTapBas_BlocksNull() =>
        AssertThat.Invoking(() => TapFile.CreateTapBas("game", null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void OperatorPlus()
    {
        var file1 = TapFile.CreateCode("file1", 0x8000, [0xF3, 0xAF]);
        var file2 = TapFile.CreateCode("file2", 0xC000, [0x00, 0x01, 0x02]);

        var combined = file1 + file2;
        combined.Blocks.Should().HaveCount(4);
        combined.Blocks[0].Should().BeOfType<HeaderBlock>().And.Value.Filename.Should().Equal("file1");
        combined.Blocks[1].Should().BeOfType<DataBlock>();
        combined.Blocks[2].Should().BeOfType<HeaderBlock>().And.Value.Filename.Should().Equal("file2");
        combined.Blocks[3].Should().BeOfType<DataBlock>();
    }

    [Test]
    public void CreateCode_SingleBlock()
    {
        var file = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        file.Blocks.Should().HaveCount(2);

        var header = file.Blocks[0].Should().BeOfType<HeaderBlock>().Value;
        header.HeaderType.Should().Equal(TapHeaderType.Code);
        header.Filename.Should().Equal("test");
        header.Location.Should().Equal(0x8000);
        header.DataBlockLength.Should().Equal(2);

        file.Blocks[1].Should().BeOfType<DataBlock>();
    }

    [Test]
    public void CreateCode_MultipleBlocks()
    {
        var file = TapFile.CreateCode("multi", (0x8000, [0xF3, 0xAF]), (0xC000, [0x00, 0x01]));

        file.Blocks.Should().HaveCount(4);

        var header1 = file.Blocks[0].Should().BeOfType<HeaderBlock>().Value;
        header1.HeaderType.Should().Equal(TapHeaderType.Code);
        header1.Filename.Should().Equal("multi");
        header1.Location.Should().Equal(0x8000);

        var header2 = file.Blocks[2].Should().BeOfType<HeaderBlock>().Value;
        header2.HeaderType.Should().Equal(TapHeaderType.Code);
        header2.Filename.Should().Equal("multi");
        header2.Location.Should().Equal(0xC000);
    }

    [Test]
    public void CreateLoader_WithoutEntryPoint()
    {
        var file = TapFile.CreateLoader("loader", (0x8000, [0xF3, 0xAF]));

        // Should have: loader header, loader data, code header, code data.
        file.Blocks.Should().HaveCount(4);

        var loaderHeader = file.Blocks[0].Should().BeOfType<HeaderBlock>().Value;
        loaderHeader.HeaderType.Should().Equal(TapHeaderType.Program);
        loaderHeader.Filename.Should().Equal("loader");

        file.Blocks[1].Should().BeOfType<DataBlock>();

        var codeHeader = file.Blocks[2].Should().BeOfType<HeaderBlock>().Value;
        codeHeader.HeaderType.Should().Equal(TapHeaderType.Code);
        codeHeader.Filename.Should().Equal("loader");
        codeHeader.Location.Should().Equal(0x8000);

        file.Blocks[3].Should().BeOfType<DataBlock>();
    }

    [Test]
    public void CreateLoader_WithEntryPoint()
    {
        var file = TapFile.CreateLoader("loader", 0x8000, (0x8000, [0xF3, 0xAF]));

        file.Blocks.Should().HaveCount(4);

        var loaderHeader = file.Blocks[0].Should().BeOfType<HeaderBlock>().Value;
        loaderHeader.HeaderType.Should().Equal(TapHeaderType.Program);
    }

    [Test]
    public void CreateLoader_MultipleCodeBlocks()
    {
        var file = TapFile.CreateLoader("loader", (0x8000, [0xF3]), (0xC000, [0xAF]));

        // Should have: loader header, loader data, code1 header, code1 data, code2 header, code2 data.
        file.Blocks.Should().HaveCount(6);
    }

    [Test]
    public void CreateTapBas_WithoutEntryPoint()
    {
        var file = TapFile.CreateTapBas("test", 0x8000, [0xF3, 0xAF]);

        // Should have: loader header, loader data, code header, code data.
        file.Blocks.Should().HaveCount(4);

        var loaderHeader = file.Blocks[0].Should().BeOfType<HeaderBlock>().Value;
        loaderHeader.HeaderType.Should().Equal(TapHeaderType.Program);
        loaderHeader.Filename.Should().Equal("loader");

        file.Blocks[1].Should().BeOfType<DataBlock>();

        var codeHeader = file.Blocks[2].Should().BeOfType<HeaderBlock>().Value;
        codeHeader.HeaderType.Should().Equal(TapHeaderType.Code);
        codeHeader.Filename.Should().Equal("test");
        codeHeader.Location.Should().Equal(0x8000);

        file.Blocks[3].Should().BeOfType<DataBlock>();
    }

    [Test]
    public void CreateTapBas_WithEntryPoint()
    {
        var file = TapFile.CreateTapBas("test", 0x8000, [0xF3, 0xAF], 0x8000);

        file.Blocks.Should().HaveCount(4);

        var loaderHeader = file.Blocks[0].Should().BeOfType<HeaderBlock>().Value;
        loaderHeader.HeaderType.Should().Equal(TapHeaderType.Program);
    }

    [Test]
    public void TryLoadInto_ReturnsFalse_WhenFirstBlockIsNotHeader()
    {
        var dataBlock = DataBlock.Create([0xF3, 0xAF]);

        // Create a file that starts with a data block instead of a header.
        TapBlock[] blocks = [dataBlock, dataBlock];
        var brokenTapFile = CreateTapFileFromBlocks(blocks);

        var memory = new byte[65536];
        brokenTapFile.TryLoadInto(memory).Should().BeFalse();
    }

    [Test]
    public void TryLoadInto_ReturnsFalse_WhenSecondBlockIsNotData()
    {
        var headerBlock = HeaderBlock.CreateCode("test", 0x8000, 2);
        var blocks = new TapBlock[] { headerBlock, headerBlock };
        var brokenTapFile = CreateTapFileFromBlocks(blocks);

        var memory = new byte[65536];
        brokenTapFile.TryLoadInto(memory).Should().BeFalse();
    }

    [Test]
    public void LoadInto_Throws_WhenFirstBlockIsNotHeader()
    {
        var dataBlock = DataBlock.Create([0xF3, 0xAF]);
        var blocks = new TapBlock[] { dataBlock, dataBlock };
        var brokenTapFile = CreateTapFileFromBlocks(blocks);

        var memory = new byte[65536];
        brokenTapFile.Invoking(f => f.LoadInto(memory)).Should().Throw<IOException>()
            .Exception.Message.Should().Equal("Missing header block when loading TAP file.");
    }

    [Test]
    public void LoadInto_Throws_WhenSecondBlockIsNotData()
    {
        var headerBlock = HeaderBlock.CreateCode("test", 0x8000, 2);
        var blocks = new TapBlock[] { headerBlock, headerBlock };
        var brokenTapFile = CreateTapFileFromBlocks(blocks);

        var memory = new byte[65536];
        brokenTapFile.Invoking(f => f.LoadInto(memory)).Should().Throw<IOException>()
            .Exception.Message.Should().Equal("Missing data block after header when loading TAP file.");
    }

    [Pure]
    private static TapFile CreateTapFileFromBlocks(TapBlock[] blocks)
    {
        // Write blocks to a stream and read back via TapFormat to create a valid TapFile.
        // Since the TapFile constructors are internal, use the internal constructor approach.
        // However, TapFile(params TapBlock[]) is internal, so we can't call it directly.
        // Instead, we can write the blocks to a stream and read back.
        using var stream = new MemoryStream();
        foreach (var block in blocks)
        {
            block.Header.Write(stream);
            block.Write(stream);
            block.Trailer.Write(stream);
        }

        stream.Position = 0;
        return TapFormat.Instance.Read(stream);
    }
}