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
        z80.Header.InterruptFlipFlop.Should().BeTrue();

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
}