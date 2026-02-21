using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Nex;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class NexFormatTests
{
    [Test]
    public void Instance()
    {
        NexFormat.Instance.Name.Should().Equal("NEX");
        NexFormat.Instance.FileExtension.Should().Equal("nex");
    }

    [Test]
    public void Read_MinimalV12_NoBanks()
    {
        var data = CreateMinimalNexData("V1.2", loadScreens: 0, banks: []);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.Header.Magic.Should().Equal("Next");
        file.Header.VersionString.Should().Equal("V1.2");
        file.Header.Version.Should().Equal(NexVersion.V12);
        file.Palette.Should().BeNull();
        file.Screens.Should().HaveCount(0);
        file.CopperCode.Should().BeNull();
        file.Banks.Should().HaveCount(0);
    }

    [Test]
    public void Read_WithSingleBank()
    {
        var bankData = new byte[16384];
        bankData[0] = 0xAA;
        bankData[16383] = 0xBB;

        var data = CreateMinimalNexData("V1.2", loadScreens: 0, banks: [(5, bankData)]);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.Banks.Should().HaveCount(1);
        file.Banks[0].BankNumber.Should().Equal(5);
        file.Banks[0].Data[0].Should().Equal(0xAA);
        file.Banks[0].Data[16383].Should().Equal(0xBB);
    }

    [Test]
    public void Read_WithMultipleBanks()
    {
        var bank5Data = new byte[16384];
        bank5Data[0] = 0x05;
        var bank2Data = new byte[16384];
        bank2Data[0] = 0x02;
        var bank0Data = new byte[16384];
        bank0Data[0] = 0x00;

        var data = CreateMinimalNexData("V1.2", loadScreens: 0, banks: [(5, bank5Data), (2, bank2Data), (0, bank0Data)]);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.Banks.Should().HaveCount(3);
        file.Banks[0].BankNumber.Should().Equal(5);
        file.Banks[0].Data[0].Should().Equal(0x05);
        file.Banks[1].BankNumber.Should().Equal(2);
        file.Banks[1].Data[0].Should().Equal(0x02);
        file.Banks[2].BankNumber.Should().Equal(0);
        file.Banks[2].Data[0].Should().Equal(0x00);
    }

    [Test]
    public void Read_WithUlaScreen()
    {
        var screenData = new byte[6912];
        screenData[0] = 0xFF;

        var data = CreateMinimalNexData("V1.2", loadScreens: 0b1000_0010, banks: [], screenBlocks: [screenData]);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.Palette.Should().BeNull();
        file.Screens.Should().HaveCount(1);
        file.Screens[0].Type.Should().Equal(NexScreenType.Ula);
        file.Screens[0].Data[0].Should().Equal(0xFF);
    }

    [Test]
    public void Read_WithLayer2ScreenAndPalette()
    {
        var paletteData = new byte[512];
        paletteData[0] = 0xE0;
        var screenData = new byte[49152];
        screenData[0] = 0xAB;

        var data = CreateMinimalNexData("V1.2", loadScreens: 0b0000_0001, banks: [], paletteData: paletteData, screenBlocks: [screenData]);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.Palette.Should().NotBeNull();
        file.Palette![0].Should().Equal(0xE0);
        file.Screens.Should().HaveCount(1);
        file.Screens[0].Type.Should().Equal(NexScreenType.Layer2);
        file.Screens[0].Data[0].Should().Equal(0xAB);
    }

    [Test]
    public void Read_InvalidMagic()
    {
        var data = new byte[512];
        data[0] = (byte)'B';
        data[1] = (byte)'a';
        data[2] = (byte)'d';
        data[3] = (byte)'!';

        using var stream = new MemoryStream(data);
        AssertThat.Invoking(() => NexFormat.Instance.Read(stream))
            .Should().Throw<InvalidOperationException>()
            .Exception.Message.Should().Match(".*magic.*");
    }

    [Test]
    public void Read_Registers()
    {
        var data = CreateMinimalNexData("V1.2", loadScreens: 0, banks: [], sp: 0x5B76, pc: 0x8000);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.Registers.SP.Should().Equal(0x5B76);
        file.Registers.PC.Should().Equal(0x8000);
        file.Registers.AF.Should().Equal(0);
    }

    [Test]
    public void RoundTrip_MinimalNoBanks()
    {
        var data = CreateMinimalNexData("V1.2", loadScreens: 0, banks: []);

        using var readStream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(readStream);

        var written = NexFormat.Instance.Write(file);
        written.Should().SequenceEqual(data);
    }

    [Test]
    public void RoundTrip_WithBanks()
    {
        var bank5Data = new byte[16384];
        bank5Data[0] = 0x05;
        var bank2Data = new byte[16384];
        bank2Data[0] = 0x02;

        var data = CreateMinimalNexData("V1.2", loadScreens: 0, banks: [(5, bank5Data), (2, bank2Data)]);

        using var readStream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(readStream);

        var written = NexFormat.Instance.Write(file);
        written.Should().SequenceEqual(data);
    }

    [Test]
    public void RoundTrip_WithScreenAndPalette()
    {
        var paletteData = new byte[512];
        paletteData[0] = 0xE0;
        var screenData = new byte[49152];
        screenData[0] = 0xAB;

        var data = CreateMinimalNexData("V1.2", loadScreens: 0b0000_0001, banks: [], paletteData: paletteData, screenBlocks: [screenData]);

        using var readStream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(readStream);

        var written = NexFormat.Instance.Write(file);
        written.Should().SequenceEqual(data);
    }

    [Test]
    public void Write_ThrowsForWrongFileType()
    {
        var tapFile = TapFile.CreateCode("test", 0, [0xF3, 0xAF]);

        using var output = new MemoryStream();
        AssertThat.Invoking(() => NexFormat.Instance.Write(tapFile, output))
            .Should().Throw<ArgumentException>();
    }

    [Test]
    public void Read_NoPaletteBlock_Flag()
    {
        var screenData = new byte[49152];
        var data = CreateMinimalNexData("V1.2", loadScreens: 0b1000_0001, banks: [], screenBlocks: [screenData]);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.Palette.Should().BeNull();
        file.Screens.Should().HaveCount(1);
        file.Screens[0].Type.Should().Equal(NexScreenType.Layer2);
    }

    private static byte[] CreateMinimalNexData(
        string version,
        byte loadScreens,
        (int bank, byte[] data)[] banks,
        byte[]? paletteData = null,
        byte[][]? screenBlocks = null,
        ushort sp = 0,
        ushort pc = 0)
    {
        using var stream = new MemoryStream();

        var header = new byte[512];
        header[0] = (byte)'N';
        header[1] = (byte)'e';
        header[2] = (byte)'x';
        header[3] = (byte)'t';
        for (var i = 0; i < version.Length; i++)
        {
            header[4 + i] = (byte)version[i];
        }

        header[9] = (byte)banks.Length;
        header[10] = loadScreens;

        header[12] = (byte)(sp & 0xFF);
        header[13] = (byte)(sp >> 8);
        header[14] = (byte)(pc & 0xFF);
        header[15] = (byte)(pc >> 8);

        foreach (var (bank, _) in banks)
        {
            header[18 + bank] = 1;
        }

        stream.Write(header);

        if (paletteData != null)
        {
            stream.Write(paletteData);
        }

        if (screenBlocks != null)
        {
            foreach (var screen in screenBlocks)
            {
                stream.Write(screen);
            }
        }

        foreach (var bankNumber in NexHeader.BankOrder)
        {
            var bankEntry = banks.FirstOrDefault(b => b.bank == bankNumber);
            if (bankEntry.data != null)
            {
                stream.Write(bankEntry.data);
            }
        }

        return stream.ToArray();
    }
}