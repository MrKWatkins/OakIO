using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Sna;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class SnaFormatTests : ZXSpectrumTestFixture
{
    [Test]
    public void Read_48k()
    {
        using var monty = OpenResource(Resources.AufWiedersehenMontySna);

        var file = SnaFormat.Instance.Read(monty);
        file.Format.Should().BeTheSameInstanceAs(SnaFormat.Instance);

        var snaFile = file.Should().BeOfType<Sna48kFile>().Value;
        snaFile.Ram.Length.Should().Equal(49152);
        AssertMontyRegisters(snaFile);
    }

    [Test]
    public void Read_48k_bytes()
    {
        using var monty = OpenResource(Resources.AufWiedersehenMontySna);
        using var ms = new MemoryStream();
        monty.CopyTo(ms);
        var bytes = ms.ToArray();

        var file = SnaFormat.Instance.Read(bytes);
        file.Format.Should().BeTheSameInstanceAs(SnaFormat.Instance);

        var snaFile = file.Should().BeOfType<Sna48kFile>().Value;
        AssertMontyRegisters(snaFile);
    }

    [Test]
    public async Task RoundTrip_48k()
    {
        await using var monty = OpenResource(Resources.AufWiedersehenMontySna);
        using var ms = new MemoryStream();
        await monty.CopyToAsync(ms);
        var expected = ms.ToArray();

        ms.Position = 0;
        var file = await SnaFormat.Instance.ReadAsync(ms);

        var actual = await WriteToBytesAsync(file);

        actual.Should().SequenceEqual(expected);
    }

    [Test]
    public async Task RoundTrip_128k()
    {
        var random = TestContext.CurrentContext.Random;

        var header = new byte[27];
        random.NextBytes(header);

        var banks = new byte[8][];
        for (var bank = 0; bank < 8; bank++)
        {
            banks[bank] = new byte[16384];
            random.NextBytes(banks[bank]);
        }

        var footer = new byte[4];
        footer.SetUInt16(0, 0x8000);    // PC.
        footer[2] = 0x10;               // Port 0x7FFD; paged bank = 0.
        footer[3] = 0;                  // TR-DOS ROM not paged.
        var pagedBank = footer[2] & 0x07;

        var contents = new List<byte>();
        contents.AddRange(header);
        contents.AddRange(banks[5]);
        contents.AddRange(banks[2]);
        contents.AddRange(banks[pagedBank]);
        contents.AddRange(footer);
        foreach (var bank in new byte[] { 0, 1, 3, 4, 6, 7 })
        {
            if (bank == pagedBank)
            {
                continue;
            }

            contents.AddRange(banks[bank]);
        }
        var expected = contents.ToArray();

        using var input = new MemoryStream(expected);
        var file = await SnaFormat.Instance.ReadAsync(input);

        var snaFile = file.Should().BeOfType<Sna128kFile>().Value;
        snaFile.Port7FFD.Should().Equal(0x10);
        snaFile.GetBank(5).ToArray().Should().SequenceEqual(banks[5]);

        var actual = await WriteToBytesAsync(file);
        actual.Should().SequenceEqual(expected);
    }

    [Test]
    public void LoadInto_48k()
    {
        using var monty = OpenResource(Resources.AufWiedersehenMontySna);

        var file = SnaFormat.Instance.Read(monty);
        var memory = new byte[65536];
        file.TryLoadInto(memory).Should().BeTrue();
    }

    [Test]
    public void Write_ThrowsForWrongFileType()
    {
        var tapFile = TapFile.CreateCode("test", 0, [0xF3, 0xAF]);

        using var output = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(output);
        AssertThat.Invoking(() => SnaFormat.Instance.WriteAsync(tapFile, writer))
            .Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_48k()
    {
        var memory = new byte[65536];
        TestContext.CurrentContext.Random.NextBytes(memory.AsSpan()[16384..]);

        var snapshot = Sna48kFile.Create(memory);
        snapshot.Header.Registers.PC = 0x1234;
        snapshot.Header.Registers.SP = 0x5B76;

        var actual = new byte[65536];
        snapshot.TryLoadInto(actual).Should().BeTrue();

        actual.Should().SequenceEqual(memory);
    }

    private static async Task<byte[]> WriteToBytesAsync(SnaFile file)
    {
        using var stream = new MemoryStream();
        using (var writer = new SyncStreamBinaryWriter(stream))
        {
            await SnaFormat.Instance.WriteAsync(file, writer);
        }

        return stream.ToArray();
    }

    private static void AssertMontyRegisters(SnaFile file)
    {
        file.Registers.PC.Should().Equal(0x0038);
        file.Registers.AF.Should().Equal(0x0044);
        file.Registers.BC.Should().Equal(0x0000);
        file.Registers.DE.Should().Equal(0xF23E);
        file.Registers.HL.Should().Equal(0x5C09);
        file.Registers.IX.Should().Equal(0xA46F);
        file.Registers.IY.Should().Equal(0x0000);
        file.Registers.IR.Should().Equal(0x133F);
        file.Registers.SP.Should().Equal(0x5B76);
        file.Registers.Shadow.AF.Should().Equal(0x0314);
        file.Registers.Shadow.BC.Should().Equal(0x0345);
        file.Registers.Shadow.DE.Should().Equal(0x023E);
        file.Registers.Shadow.HL.Should().Equal(0x5976);
        file.Header.IFF2.Should().BeFalse();
        file.Header.InterruptMode.Should().Equal(1);
        file.Header.BorderColour.Should().Equal(ZXColour.Black);
    }
}