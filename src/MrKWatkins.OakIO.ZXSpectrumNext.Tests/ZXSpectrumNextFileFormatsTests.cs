using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using MrKWatkins.OakIO.ZXSpectrumNext.Snapshot.Nex;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests;

public sealed class ZXSpectrumNextFileFormatsTests
{
    [Test]
    public void AllFormats()
    {
        ZXSpectrumNextFileFormats.AllFormats.Should().SequenceEqual([.. ZXSpectrumFileFormat.AllFormats, NexFormat.Instance]);
        ZXSpectrumFileFormat.AllFormats.Select(format => format.FileExtension).Should().NotContain("nex");
    }

    [Test]
    public void AllFormats_LoadNex()
    {
        var original = NexFile.CreateCode([(2, [0xF3, 0xC9])], pc: 0x8000, sp: 0xFFFE);
        using var stream = new MemoryStream(original.ToByteArray());

        var file = IOFileFormat.Load("test.nex", stream, ZXSpectrumNextFileFormats.AllFormats);

        var nex = file.Should().BeOfType<NexFile>().Value;
        nex.Header.PC.Should().Equal((ushort)0x8000);
        nex.Header.SP.Should().Equal((ushort)0xFFFE);
        nex.Banks[0].BankNumber.Should().Equal(2);
        nex.Banks[0].Data.Take(2).Should().SequenceEqual(0xF3, 0xC9);
    }

    [Test]
    public async Task AllFormats_LoadNexAsync()
    {
        var original = NexFile.CreateCode([], pc: 0x8123, sp: 0xFFFE);
        using var stream = new MemoryStream(original.ToByteArray());

        var file = await IOFileFormat.LoadAsync("test.nex", stream, ZXSpectrumNextFileFormats.AllFormats);

        file.Should().BeOfType<NexFile>().Value.Header.PC.Should().Equal((ushort)0x8123);
    }

    [Test]
    public void AllFormats_LoadSpectrum()
    {
        var original = TapFile.CreateCode("test", 0x8000, [0xF3, 0xC9]);
        using var stream = new MemoryStream(original.ToByteArray());

        var file = IOFileFormat.Load("test.tap", stream, ZXSpectrumNextFileFormats.AllFormats);

        file.Should().BeOfType<TapFile>().Value.ToByteArray().Should().SequenceEqual(original.ToByteArray());
    }
}