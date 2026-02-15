using MrKWatkins.OakIO.ZXSpectrum.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tap;

public sealed class TapFileTests : TapTestFixture
{
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
        var file = TapFile.CreateCode("multi", ((ushort)0x8000, new byte[] { 0xF3, 0xAF }), ((ushort)0xC000, new byte[] { 0x00, 0x01 }));

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
        var file = TapFile.CreateLoader("loader", ((ushort)0x8000, new byte[] { 0xF3, 0xAF }));

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
        var file = TapFile.CreateLoader("loader", (ushort)0x8000, ((ushort)0x8000, new byte[] { 0xF3, 0xAF }));

        file.Blocks.Should().HaveCount(4);

        var loaderHeader = file.Blocks[0].Should().BeOfType<HeaderBlock>().Value;
        loaderHeader.HeaderType.Should().Equal(TapHeaderType.Program);
    }

    [Test]
    public void CreateLoader_MultipleCodeBlocks()
    {
        var file = TapFile.CreateLoader("loader", ((ushort)0x8000, new byte[] { 0xF3 }), ((ushort)0xC000, new byte[] { 0xAF }));

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
        var file = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        // Create a file that starts with a data block instead of a header.
        var brokenFile = file + TapFile.CreateCode("x", 0, [0]);
        var blocks = new[] { dataBlock, dataBlock };
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