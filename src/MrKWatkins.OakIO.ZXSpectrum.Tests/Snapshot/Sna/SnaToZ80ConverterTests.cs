using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Sna;

public sealed class SnaToZ80ConverterTests
{
    [Test]
    public void Convert()
    {
        var memory = new byte[65536];
        memory[0x8000] = 0xF3;
        memory[0x8001] = 0xAF;

        var sna = Sna48kFile.Create(memory);
        sna.Header.BorderColour = ZXColour.Red;
        sna.Header.InterruptMode = 2;
        sna.Header.IFF2 = true;
        sna.Registers.AF = 0x1234;
        sna.Registers.BC = 0x5678;
        sna.Registers.DE = 0x9ABC;
        sna.Registers.HL = 0xDEF0;
        sna.Registers.IX = 0x1111;
        sna.Registers.IY = 0x2222;
        sna.Registers.PC = 0x8000;
        sna.Registers.SP = 0xFF00;
        sna.Registers.IR = 0x3F00;
        sna.Registers.Shadow.AF = 0xAAAA;
        sna.Registers.Shadow.BC = 0xBBBB;
        sna.Registers.Shadow.DE = 0xCCCC;
        sna.Registers.Shadow.HL = 0xDDDD;

        var z80 = new SnaToZ80Converter().Convert(sna);

        z80.Should().BeOfType<Z80V2File>();

        z80.Header.BorderColour.Should().Equal(ZXColour.Red);
        z80.Header.InterruptMode.Should().Equal(2);
        z80.Header.IFF2.Should().BeTrue();
        z80.Header.IFF1.Should().BeTrue();

        z80.Registers.AF.Should().Equal(0x1234);
        z80.Registers.BC.Should().Equal(0x5678);
        z80.Registers.DE.Should().Equal(0x9ABC);
        z80.Registers.HL.Should().Equal(0xDEF0);
        z80.Registers.IX.Should().Equal(0x1111);
        z80.Registers.IY.Should().Equal(0x2222);
        z80.Registers.PC.Should().Equal(0x8000);
        z80.Registers.SP.Should().Equal(0xFF00);
        z80.Registers.IR.Should().Equal(0x3F00);
        z80.Registers.Shadow.AF.Should().Equal(0xAAAA);
        z80.Registers.Shadow.BC.Should().Equal(0xBBBB);
        z80.Registers.Shadow.DE.Should().Equal(0xCCCC);
        z80.Registers.Shadow.HL.Should().Equal(0xDDDD);

        var z80Memory = new byte[65536];
        z80.TryLoadInto(z80Memory).Should().BeTrue();
        z80Memory[0x8000].Should().Equal(0xF3);
        z80Memory[0x8001].Should().Equal(0xAF);
    }

    [Test]
    public void Convert_TrDosRom()
    {
        var sna = Sna128kFile.Create(Enumerable.Repeat<ReadOnlyMemory<byte>>(new byte[16384], 8).ToArray(), trDosRomPaged: true);
        AssertThat.Invoking(() => new SnaToZ80Converter().Convert(sna)).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Convert_128k([Range(0, 7)] int selected)
    {
        var banks = Enumerable.Range(0, 8).Select(bank => (ReadOnlyMemory<byte>)Enumerable.Repeat((byte)(bank + 17), 16384).ToArray()).ToArray();
        var sna = Sna128kFile.Create(banks, (byte)(0x38 | selected));
        sna.Registers.PC = 0x8123;
        sna.Registers.SP = 0xBFFE;
        sna.Header.BorderColour = ZXColour.Green;
        sna.Header.IFF2 = true;
        sna.Header.InterruptMode = 2;
        var z80 = IOFileConversion.Convert<Z80File>(sna).Should().BeOfType<Z80V2File>().Value;
        z80.Port7FFD.Should().Equal((byte)(0x38 | selected));
        z80.Header.BorderColour.Should().Equal(ZXColour.Green);
        z80.Header.IFF1.Should().BeTrue();
        z80.Header.IFF2.Should().BeTrue();
        z80.Header.InterruptMode.Should().Equal((byte)2);
        z80.Registers.PC.Should().Equal((ushort)0x8123);
        z80.Registers.SP.Should().Equal((ushort)0xBFFE);
        var restored = IOFileConversion.Convert<SnaFile>(Z80Format.Instance.Read(z80.ToByteArray())).Should().BeOfType<Sna128kFile>().Value;
        restored.Port7FFD.Should().Equal(sna.Port7FFD);
        restored.Registers.PC.Should().Equal((ushort)0x8123);
        restored.Registers.SP.Should().Equal((ushort)0xBFFE);
        for (var bank = 0; bank < 8; bank++)
        {
            z80.GetBank(bank).Should().SequenceEqual(banks[bank].ToArray());
            restored.GetBank(bank).ToArray().Should().SequenceEqual(banks[bank].ToArray());
        }
    }
}