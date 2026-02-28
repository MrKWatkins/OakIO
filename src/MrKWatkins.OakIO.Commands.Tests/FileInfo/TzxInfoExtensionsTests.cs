using MrKWatkins.OakIO.Commands.FileInfo;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

namespace MrKWatkins.OakIO.Commands.Tests.FileInfo;

public sealed class TzxInfoExtensionsTests
{
    [Test]
    public void ToInfoSections_ReturnsSingleBlocksSection()
    {
        var tzx = ReadTzx([0x10, 0xE8, 0x03, 0x04, 0x00, 0xFF, 0x01, 0x02, 0x00]);
        var sections = tzx.ToInfoSections();
        sections.Single(s => s.Title == "Blocks").Should().NotBeNull();
    }

    [Test]
    public void ToInfoSections_ArchiveInfo_CreatesArchiveInfoSection()
    {
        // 0x32 = ArchiveInfo; header: length=7 (LE), count=1; entry: type=0 (Full Title), len=4, "test"
        var tzx = ReadTzx([0x32, 0x07, 0x00, 0x01, 0x00, 0x04, .."test"u8]);
        var sections = tzx.ToInfoSections();
        var archiveInfo = sections.Single(s => s.Title == "Archive Info");
        archiveInfo.Properties.Should().HaveCount(1);
        archiveInfo.Properties[0].Name.Should().Equal("Full Title");
        archiveInfo.Properties[0].Value.Should().Equal("test");
    }

    [Test]
    public void ToInfoSections_StandardSpeedData()
    {
        var tzx = ReadTzx([0x10, 0xE8, 0x03, 0x04, 0x00, 0xFF, 0x01, 0x02, 0x00]);
        var items = tzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items.Should().HaveCount(1);
        items[0].Title.Should().Equal("Standard Speed Data");
        items[0].Properties.Single(p => p.Name == "Length").Value.Should().Equal("4");
        items[0].Properties.Single(p => p.Name == "Pause After").Value.Should().Equal("1000 ms");
    }

    [Test]
    public void ToInfoSections_PureTone()
    {
        // 0x12 = PureTone; pulse length=2168 (0x78,0x08), num pulses=8063 (0x7F,0x1F)
        var tzx = ReadTzx([0x12, 0x78, 0x08, 0x7F, 0x1F]);
        var items = tzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items[0].Title.Should().Equal("Pure Tone");
        items[0].Properties.Single(p => p.Name == "Pulse Count").Value.Should().Equal("8063");
        items[0].Properties.Single(p => p.Name == "Pulse Length").Value.Should().Equal("2168 T-States");
    }

    [Test]
    public void ToInfoSections_PulseSequence()
    {
        // 0x13 = PulseSequence; 2 pulses
        var tzx = ReadTzx([0x13, 0x02, 0x9B, 0x02, 0xDF, 0x02]);
        var items = tzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items[0].Title.Should().Equal("Pulse Sequence");
        items[0].Properties.Single(p => p.Name == "Pulse Count").Value.Should().Equal("2");
    }

    [Test]
    public void ToInfoSections_Pause()
    {
        // 0x20 = Pause; 500ms (0xF4, 0x01)
        var tzx = ReadTzx([0x20, 0xF4, 0x01]);
        var items = tzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items[0].Title.Should().Equal("Pause");
        items[0].Properties.Single(p => p.Name == "Duration").Value.Should().Equal("500 ms");
    }

    [Test]
    public void ToInfoSections_TextDescription()
    {
        // 0x30 = TextDescription; length=4, "test"
        var tzx = ReadTzx([0x30, 0x04, .."test"u8]);
        var items = tzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items[0].Title.Should().Equal("Text Description");
        items[0].Properties.Single(p => p.Name == "Text").Value.Should().Equal("test");
    }

    [Test]
    public void ToInfoSections_StopTheTapeIf48K()
    {
        // 0x2A = StopTheTapeIf48K; 4 zero bytes
        var tzx = ReadTzx([0x2A, 0x00, 0x00, 0x00, 0x00]);
        var items = tzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items[0].Title.Should().Equal("Stop Tape (48K)");
    }

    [Test]
    public void ToInfoSections_Loop()
    {
        // LoopStart (0x24, 2 reps) + StandardSpeedData + LoopEnd (0x25)
        var tzx = ReadTzx(
            [0x24, 0x02, 0x00],
            [0x10, 0xE8, 0x03, 0x04, 0x00, 0xFF, 0x01, 0x02, 0x00],
            [0x25]);
        var items = tzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items.Should().HaveCount(1);
        items[0].Title.Should().Equal("Loop");
        items[0].Properties.Single(p => p.Name == "Repetitions").Value.Should().Equal("2");
        var nestedBlocks = items[0].Sections.Single(s => s.Title == "Blocks");
        nestedBlocks.Items.Should().HaveCount(1);
        nestedBlocks.Items[0].Title.Should().Equal("Standard Speed Data");
    }

    [Test]
    public void ToInfoSections_Group()
    {
        // GroupStart (0x21, name "grp") + Pause + GroupEnd (0x22)
        var tzx = ReadTzx(
            [0x21, 0x03, .."grp"u8],
            [0x20, 0xF4, 0x01],
            [0x22]);
        var items = tzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items.Should().HaveCount(1);
        items[0].Title.Should().Equal("Group: grp");
        items[0].Sections.Single(s => s.Title == "Blocks").Items.Should().HaveCount(1);
    }

    [Pure]
    private static TzxFile ReadTzx(params byte[][] blockByteSets)
    {
        using var stream = new MemoryStream();
        stream.Write("ZXTape!\x1A"u8);
        stream.WriteByte(0x01);
        stream.WriteByte(0x14);
        foreach (var block in blockByteSets)
        {
            stream.Write(block);
        }
        stream.Position = 0;
        return TzxFormat.Instance.Read(stream);
    }
}