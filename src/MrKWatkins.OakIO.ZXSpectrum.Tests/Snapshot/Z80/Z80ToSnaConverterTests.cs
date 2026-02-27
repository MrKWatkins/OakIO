using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Z80;

public sealed class Z80ToSnaConverterTests
{
    [Test]
    public void Convert()
    {
        var memory = new byte[65536];
        memory[0x8000] = 0xF3;
        memory[0x8001] = 0xAF;

        var z80 = Z80V1File.Create48k(memory);
        z80.Header.BorderColour = ZXColour.Blue;
        z80.Header.InterruptMode = 1;
        z80.Header.IFF2 = true;
        z80.Registers.AF = 0x1234;
        z80.Registers.BC = 0x5678;
        z80.Registers.DE = 0x9ABC;
        z80.Registers.HL = 0xDEF0;
        z80.Registers.IX = 0x1111;
        z80.Registers.IY = 0x2222;
        z80.Registers.PC = 0x8000;
        z80.Registers.SP = 0xFF00;
        z80.Registers.IR = 0x3F00;
        z80.Registers.Shadow.AF = 0xAAAA;
        z80.Registers.Shadow.BC = 0xBBBB;
        z80.Registers.Shadow.DE = 0xCCCC;
        z80.Registers.Shadow.HL = 0xDDDD;

        var sna = new Z80ToSnaConverter().Convert(z80);

        sna.Should().BeOfType<Sna48kFile>();

        sna.Header.BorderColour.Should().Equal(ZXColour.Blue);
        sna.Header.InterruptMode.Should().Equal(1);
        sna.Header.IFF2.Should().BeTrue();

        sna.Registers.AF.Should().Equal(0x1234);
        sna.Registers.BC.Should().Equal(0x5678);
        sna.Registers.DE.Should().Equal(0x9ABC);
        sna.Registers.HL.Should().Equal(0xDEF0);
        sna.Registers.IX.Should().Equal(0x1111);
        sna.Registers.IY.Should().Equal(0x2222);
        sna.Registers.PC.Should().Equal(0x8000);
        sna.Registers.SP.Should().Equal(0xFF00);
        sna.Registers.IR.Should().Equal(0x3F00);
        sna.Registers.Shadow.AF.Should().Equal(0xAAAA);
        sna.Registers.Shadow.BC.Should().Equal(0xBBBB);
        sna.Registers.Shadow.DE.Should().Equal(0xCCCC);
        sna.Registers.Shadow.HL.Should().Equal(0xDDDD);

        var snaMemory = new byte[65536];
        sna.TryLoadInto(snaMemory).Should().BeTrue();
        snaMemory[0x8000].Should().Equal(0xF3);
        snaMemory[0x8001].Should().Equal(0xAF);
    }
}