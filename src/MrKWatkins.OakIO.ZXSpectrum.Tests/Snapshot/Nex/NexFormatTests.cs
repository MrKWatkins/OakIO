using MrKWatkins.OakIO.Binary;
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
    public void Read_WithLoResScreen()
    {
        // A LoRes screen also implies a palette block, like Layer2.
        var paletteData = new byte[512];
        var screenData = new byte[12288];
        screenData[0] = 0xCD;

        var data = CreateMinimalNexData("V1.2", loadScreens: 0b0000_0100, banks: [], paletteData: paletteData, screenBlocks: [screenData]);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.Palette.Should().NotBeNull();
        file.Screens.Should().HaveCount(1);
        file.Screens[0].Type.Should().Equal(NexScreenType.LoRes);
        file.Screens[0].Data[0].Should().Equal(0xCD);
    }

    [Test]
    public void Read_WithHiResScreen()
    {
        var screenData = new byte[12288];
        screenData[0] = 0xEF;

        var data = CreateMinimalNexData("V1.2", loadScreens: 0b0000_1000, banks: [], screenBlocks: [screenData]);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.Screens.Should().HaveCount(1);
        file.Screens[0].Type.Should().Equal(NexScreenType.HiRes);
        file.Screens[0].Data[0].Should().Equal(0xEF);
    }

    [Test]
    public void Read_WithHiColourScreen()
    {
        var screenData = new byte[12288];
        screenData[0] = 0x12;

        var data = CreateMinimalNexData("V1.2", loadScreens: 0b0001_0000, banks: [], screenBlocks: [screenData]);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.Screens.Should().HaveCount(1);
        file.Screens[0].Type.Should().Equal(NexScreenType.HiColour);
        file.Screens[0].Data[0].Should().Equal(0x12);
    }

    [TestCase(NexLoadScreenMode.Layer2x320x256, NexScreenType.Layer2x320x256)]
    [TestCase(NexLoadScreenMode.Layer2x640x256, NexScreenType.Layer2x640x256)]
    [TestCase(NexLoadScreenMode.None, NexScreenType.Layer2x320x256)]
    public void Read_WithFlags2Screen(NexLoadScreenMode mode, NexScreenType expectedType)
    {
        var screenData = new byte[81920];
        screenData[0] = 0x34;

        var data = CreateMinimalNexData("V1.3", loadScreens: 0b0100_0000, banks: [], screenBlocks: [screenData], loadScreens2: mode);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.Screens.Should().HaveCount(1);
        file.Screens[0].Type.Should().Equal(expectedType);
        file.Screens[0].Data[0].Should().Equal(0x34);
    }

    [Test]
    public void Read_WithTilemodeFlags2_ReadsPaletteWithoutLayer2OrLoRes()
    {
        var paletteData = new byte[512];
        paletteData[0] = 0x56;
        var screenData = new byte[81920];

        var data = CreateMinimalNexData(
            "V1.3",
            loadScreens: 0b0100_0000,
            banks: [],
            paletteData: paletteData,
            screenBlocks: [screenData],
            loadScreens2: NexLoadScreenMode.Tilemode);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.Palette.Should().NotBeNull();
        file.Palette![0].Should().Equal(0x56);
    }

    [Test]
    public void Read_WithCopperCode()
    {
        var copperCode = new byte[2048];
        copperCode[0] = 0x78;

        var data = CreateMinimalNexData("V1.3", loadScreens: 0, banks: [], copperCode: copperCode);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.CopperCode.Should().NotBeNull();
        file.CopperCode![0].Should().Equal(0x78);
    }

    [Test]
    public void Read_V12_IgnoresCopperCodeFlag()
    {
        // The HasCopperCode flag is only meaningful from V1.3 onwards.
        var data = CreateMinimalNexData("V1.2", loadScreens: 0, banks: [], copperCode: new byte[2048]);

        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        file.CopperCode.Should().BeNull();
    }

    [Test]
    public async Task RoundTrip_WithCopperCode()
    {
        var copperCode = new byte[2048];
        copperCode[0] = 0x9A;

        var data = CreateMinimalNexData("V1.3", loadScreens: 0, banks: [], copperCode: copperCode);

        using var readStream = new MemoryStream(data);
        var file = await NexFormat.Instance.ReadAsync(readStream);

        var actual = await WriteToBytesAsync(file);
        actual.Should().SequenceEqual(data);
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
    public void Registers_RoundTripThroughWrite()
    {
        var data = CreateMinimalNexData("V1.2", loadScreens: 0, banks: [], sp: 0x5B76, pc: 0x8000);
        using var stream = new MemoryStream(data);
        var file = NexFormat.Instance.Read(stream);

        // The registers share the header's data, so changes must survive a write/read round-trip.
        file.Registers.PC = 0x1234;
        file.Registers.SP = 0x5678;

        var roundTripped = NexFormat.Instance.Read(file.ToByteArray());
        roundTripped.Registers.PC.Should().Equal(0x1234);
        roundTripped.Registers.SP.Should().Equal(0x5678);
    }

    [Test]
    public async Task RoundTrip_MinimalNoBanks()
    {
        var data = CreateMinimalNexData("V1.2", loadScreens: 0, banks: []);

        using var readStream = new MemoryStream(data);
        var file = await NexFormat.Instance.ReadAsync(readStream);

        var actual = await WriteToBytesAsync(file);
        actual.Should().SequenceEqual(data);
    }

    [Test]
    public async Task RoundTrip_WithBanks()
    {
        var bank5Data = new byte[16384];
        bank5Data[0] = 0x05;
        var bank2Data = new byte[16384];
        bank2Data[0] = 0x02;

        var data = CreateMinimalNexData("V1.2", loadScreens: 0, banks: [(5, bank5Data), (2, bank2Data)]);

        using var readStream = new MemoryStream(data);
        var file = await NexFormat.Instance.ReadAsync(readStream);

        var actual = await WriteToBytesAsync(file);
        actual.Should().SequenceEqual(data);
    }

    [Test]
    public async Task RoundTrip_WithScreenAndPalette()
    {
        var paletteData = new byte[512];
        paletteData[0] = 0xE0;
        var screenData = new byte[49152];
        screenData[0] = 0xAB;

        var data = CreateMinimalNexData("V1.2", loadScreens: 0b0000_0001, banks: [], paletteData: paletteData, screenBlocks: [screenData]);

        using var readStream = new MemoryStream(data);
        var file = await NexFormat.Instance.ReadAsync(readStream);

        var actual = await WriteToBytesAsync(file);
        actual.Should().SequenceEqual(data);
    }

    [Test]
    public void Write_ThrowsForWrongFileType()
    {
        var tapFile = TapFile.CreateCode("test", 0, [0xF3, 0xAF]);

        using var output = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(output);
        AssertThat.Invoking(() => NexFormat.Instance.WriteAsync(tapFile, writer))
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

    private static async Task<byte[]> WriteToBytesAsync(NexFile file)
    {
        using var stream = new MemoryStream();
        using (var writer = new SyncStreamBinaryWriter(stream))
        {
            await NexFormat.Instance.WriteAsync(file, writer);
        }

        return stream.ToArray();
    }

    [Pure]
    private static byte[] CreateMinimalNexData(
        string version,
        byte loadScreens,
        (int bank, byte[] data)[] banks,
        byte[]? paletteData = null,
        byte[][]? screenBlocks = null,
        ushort sp = 0,
        ushort pc = 0,
        NexLoadScreenMode? loadScreens2 = null,
        byte[]? copperCode = null)
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

        if (loadScreens2 != null)
        {
            header[152] = (byte)loadScreens2.Value;
        }

        if (copperCode != null)
        {
            header[153] = 1;
        }

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

        if (copperCode != null)
        {
            stream.Write(copperCode);
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