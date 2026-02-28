using MrKWatkins.OakIO.Commands.FileInfo;
using Pzx = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

namespace MrKWatkins.OakIO.Commands.Tests.FileInfo;

public sealed class PzxInfoExtensionsTests
{
    [Test]
    public void ToInfoSections_HeaderWithInfo_CreatesInfoSection()
    {
        // PZX info: first entry body is just text (type defaults to "Title"), subsequent are type\0text\0
        var data = CreatePzxData(
            CreatePzxtBlock([("Title", "Test Title")]));
        using var stream = new MemoryStream(data);
        var pzx = Pzx.PzxFormat.Instance.Read(stream);
        var sections = pzx.ToInfoSections();
        var infoSection = sections.FirstOrDefault(s => s.Title == "Info");
        infoSection.Should().NotBeNull();
        infoSection!.Items.Should().HaveCount(1);
        infoSection.Items[0].Title.Should().Equal("Title");
        infoSection.Items[0].Properties.Single(p => p.Name == "Value").Value.Should().Equal("Test Title");
    }

    [Test]
    public void ToInfoSections_EmptyHeader_NoInfoSection()
    {
        var data = CreatePzxData(CreatePzxtBlock(info: []));
        using var stream = new MemoryStream(data);
        var pzx = Pzx.PzxFormat.Instance.Read(stream);
        var sections = pzx.ToInfoSections();
        sections.Any(s => s.Title == "Info").Should().BeFalse();
    }

    [Test]
    public void ToInfoSections_PulseSequenceBlock()
    {
        // PULS block: size=4 (2 pulses × 2 bytes), 2 pulses
        var data = CreatePzxData(
            CreatePzxtBlock(info: []),
            [.."PULS"u8,
             0x04, 0x00, 0x00, 0x00, // size=4
             0x9B, 0x02, 0xDF, 0x02  // 2 pulses: 667, 735
            ]);
        using var stream = new MemoryStream(data);
        var pzx = Pzx.PzxFormat.Instance.Read(stream);
        var items = pzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items.Should().HaveCount(1);
        items[0].Title.Should().Equal("Pulse Sequence");
        items[0].Properties.Single(p => p.Name == "Pulse Count").Value.Should().Equal("2");
    }

    [Test]
    public void ToInfoSections_PauseBlock()
    {
        // PAUS block: size=4, duration=500 T-states
        var data = CreatePzxData(
            CreatePzxtBlock(info: []),
            [.."PAUS"u8,
             0x04, 0x00, 0x00, 0x00, // size=4
             0xF4, 0x01, 0x00, 0x00  // duration=500, initial pulse level=0
            ]);
        using var stream = new MemoryStream(data);
        var pzx = Pzx.PzxFormat.Instance.Read(stream);
        var items = pzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items[0].Title.Should().Equal("Pause");
        items[0].Properties.Single(p => p.Name == "Duration").Value.Should().Equal("500 T-States");
    }

    [Test]
    public void ToInfoSections_BrowsePointBlock()
    {
        // BRWS block: size=5, text="hello"
        var data = CreatePzxData(
            CreatePzxtBlock(info: []),
            [.."BRWS"u8,
             0x05, 0x00, 0x00, 0x00, // size=5
             .."hello"u8
            ]);
        using var stream = new MemoryStream(data);
        var pzx = Pzx.PzxFormat.Instance.Read(stream);
        var items = pzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items[0].Title.Should().Equal("Browse Point");
        items[0].Properties.Single(p => p.Name == "Text").Value.Should().Equal("hello");
    }

    [Test]
    public void ToInfoSections_StopBlock_Any()
    {
        // STOP block: size=2, flags=0 (any machine)
        var data = CreatePzxData(
            CreatePzxtBlock(info: []),
            [.."STOP"u8,
             0x02, 0x00, 0x00, 0x00, // size=2
             0x00, 0x00               // only48k=false
            ]);
        using var stream = new MemoryStream(data);
        var pzx = Pzx.PzxFormat.Instance.Read(stream);
        var items = pzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items[0].Title.Should().Equal("Stop");
        items[0].Properties.Single(p => p.Name == "48K Only").Value.Should().Equal("false");
    }

    [Test]
    public void ToInfoSections_StopBlock_Only48K()
    {
        // STOP block: size=2, flags=1 (48K only)
        var data = CreatePzxData(
            CreatePzxtBlock(info: []),
            [.."STOP"u8,
             0x02, 0x00, 0x00, 0x00, // size=2
             0x01, 0x00               // only48k=true
            ]);
        using var stream = new MemoryStream(data);
        var pzx = Pzx.PzxFormat.Instance.Read(stream);
        var items = pzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items[0].Properties.Single(p => p.Name == "48K Only").Value.Should().Equal("true");
    }

    [Test]
    public void ToInfoSections_DataBlock()
    {
        // DATA block: header size=12, size_in_bits=8 (1 byte of data), tail=0, zeroBitPulses=0, oneBitPulses=0
        // body = 1 byte of data = [0xFF]
        var data = CreatePzxData(
            CreatePzxtBlock(info: []),
            [.."DATA"u8,
             0x09, 0x00, 0x00, 0x00, // size=9 (8 header + 1 data byte)
             0x08, 0x00, 0x00, 0x00, // size_in_bits=8, initial_pulse_level=0
             0x00, 0x00,             // tail=0
             0x00,                   // zeroBitPulses=0
             0x00,                   // oneBitPulses=0
             0xFF                    // 1 byte data
            ]);
        using var stream = new MemoryStream(data);
        var pzx = Pzx.PzxFormat.Instance.Read(stream);
        var items = pzx.ToInfoSections().Single(s => s.Title == "Blocks").Items;
        items[0].Title.Should().Equal("Data");
        items[0].Properties.Single(p => p.Name == "Size").Value.Should().Equal("1");
    }

    [Pure]
    private static byte[] CreatePzxData(params byte[][] blocks)
    {
        using var stream = new MemoryStream();
        foreach (var block in blocks)
        {
            stream.Write(block);
        }
        return stream.ToArray();
    }

    [Pure]
    private static byte[] CreatePzxtBlock((string type, string text)[] info)
    {
        using var stream = new MemoryStream();
        // "PZXT" tag (big-endian)
        stream.Write("PZXT"u8);
        // Build body: first entry is just text\0 (type defaults to "Title"),
        // subsequent entries are type\0text\0
        using var body = new MemoryStream();
        for (var i = 0; i < info.Length; i++)
        {
            var (type, text) = info[i];
            if (i > 0)
            {
                body.Write(System.Text.Encoding.ASCII.GetBytes(type));
                body.WriteByte(0);
            }
            body.Write(System.Text.Encoding.ASCII.GetBytes(text));
            body.WriteByte(0);
        }
        var bodyBytes = body.ToArray();
        // size field = 2 (major+minor) + body
        var size = 2 + bodyBytes.Length;
        stream.Write([(byte)(size & 0xFF), (byte)((size >> 8) & 0xFF), 0, 0]);
        stream.WriteByte(0x01); // major
        stream.WriteByte(0x00); // minor
        stream.Write(bodyBytes);
        return stream.ToArray();
    }
}