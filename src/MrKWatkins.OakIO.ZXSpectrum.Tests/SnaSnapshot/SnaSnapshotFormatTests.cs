using MrKWatkins.OakIO.ZXSpectrum.SnaSnapshot;
using MrKWatkins.OakIO.ZXSpectrum.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.SnaSnapshot;

public sealed class SnaSnapshotFormatTests : ZXSpectrumTestFixture
{
    [Test]
    public void Read_48k()
    {
        using var monty = OpenResource(Resources.AufWiedersehenMontySna);

        var file = SnaSnapshotFormat.Instance.Read(monty);
        file.Format.Should().BeTheSameInstanceAs(SnaSnapshotFormat.Instance);

        var snaFile = file.Should().BeOfType<SnaSnapshot48kFile>().Value;
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

        var file = SnaSnapshotFormat.Instance.Read(bytes);
        file.Format.Should().BeTheSameInstanceAs(SnaSnapshotFormat.Instance);

        var snaFile = file.Should().BeOfType<SnaSnapshot48kFile>().Value;
        AssertMontyRegisters(snaFile);
    }

    [Test]
    public void RoundTrip_48k()
    {
        using var monty = OpenResource(Resources.AufWiedersehenMontySna);
        using var ms = new MemoryStream();
        monty.CopyTo(ms);
        var expected = ms.ToArray();

        ms.Position = 0;
        var file = SnaSnapshotFormat.Instance.Read(ms);

        var actual = SnaSnapshotFormat.Instance.Write(file);

        actual.Should().SequenceEqual(expected);
    }

    [Test]
    public void LoadInto_48k()
    {
        using var monty = OpenResource(Resources.AufWiedersehenMontySna);

        var file = SnaSnapshotFormat.Instance.Read(monty);
        var memory = new byte[65536];
        file.TryLoadInto(memory).Should().BeTrue();
    }

    [Test]
    public void Write_ThrowsForWrongFileType()
    {
        var tapFile = TapFile.CreateCode("test", 0, [0xF3, 0xAF]);

        using var output = new MemoryStream();
        AssertThat.Invoking(() => SnaSnapshotFormat.Instance.Write(tapFile, output))
            .Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_48k()
    {
        var memory = new byte[65536];
        TestContext.CurrentContext.Random.NextBytes(memory.AsSpan()[16384..]);

        var snapshot = SnaSnapshot48kFile.Create(memory);
        snapshot.Header.Registers.PC = 0x1234;
        snapshot.Header.Registers.SP = 0x5B76;

        var actual = new byte[65536];
        snapshot.TryLoadInto(actual).Should().BeTrue();

        actual.Should().SequenceEqual(memory);
    }

    private static void AssertMontyRegisters(SnaSnapshotFile file)
    {
        file.Registers.PC.Should().Equal((ushort)0x0038);
        file.Registers.AF.Should().Equal((ushort)0x0044);
        file.Registers.BC.Should().Equal((ushort)0x0000);
        file.Registers.DE.Should().Equal((ushort)0xF23E);
        file.Registers.HL.Should().Equal((ushort)0x5C09);
        file.Registers.IX.Should().Equal((ushort)0xA46F);
        file.Registers.IY.Should().Equal((ushort)0x0000);
        file.Registers.IR.Should().Equal((ushort)0x133F);
        file.Registers.SP.Should().Equal((ushort)0x5B76);
        file.Registers.Shadow.AF.Should().Equal((ushort)0x0314);
        file.Registers.Shadow.BC.Should().Equal((ushort)0x0345);
        file.Registers.Shadow.DE.Should().Equal((ushort)0x023E);
        file.Registers.Shadow.HL.Should().Equal((ushort)0x5976);
        file.Header.IFF2.Should().BeFalse();
        file.Header.InterruptMode.Should().Equal((byte)1);
        file.Header.BorderColour.Should().Equal(ZXColour.Black);
    }
}