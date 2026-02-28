using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Z80;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class Z80FormatTests : ZXSpectrumTestFixture
{
    [Test]
    public void Write_V1_ThrowsForPC0([Values] bool isCompressed)
    {
        var header = new Z80V1Header
        {
            Registers =
            {
                PC = 0
            }
        };

        var snapshot = new Z80V1File(header, new byte[16384]);

        using var memoryStream = new MemoryStream();
        // ReSharper disable once AccessToDisposedClosure
        Z80Format.Instance.Invoking(f => f.Write(snapshot, memoryStream))
            .Should().Throw<InvalidOperationException>()
            .Exception.Message.Should().Equal("PC cannot be 0 for a v1 file; a PC value of 0 is to specify a v2 or v3 file.");
    }

    [TestCase(Resources.AufWiedersehenMontyZ80V1Compressed, true)]
    [TestCase(Resources.AufWiedersehenMontyZ80V1Uncompressed, false)]
    public void Read_V1(string resource, bool expectedDataIsCompressed)
    {
        using var monty = OpenResource(resource);

        var file = Z80Format.Instance.Read(monty);
        file.Format.Should().BeTheSameInstanceAs(Z80Format.Instance);

        var v1File = file.Should().BeOfType<Z80V1File>().Value;
        v1File.Header.DataIsCompressed.Should().Equal(expectedDataIsCompressed);
        v1File.UncompressedData.Length.Should().Equal(49152);
        AssertMontyV1(v1File);
    }

    [Test]
    public void Read_V2()
    {
        using var monty = OpenResource(Resources.AufWiedersehenMontyZ80V2);

        var file = Z80Format.Instance.Read(monty);
        file.Format.Should().BeTheSameInstanceAs(Z80Format.Instance);

        var v2File = file.Should().BeOfType<Z80V2File>().Value;
        AssertMontyV2OrV3<Z80V2File, Z80V2Header>(v2File);
    }

    [Test]
    public void Read_V2_bytes()
    {
        using var monty = OpenResource(Resources.AufWiedersehenMontyZ80V2);
        var bytes = monty.ReadAllBytes();

        var file = Z80Format.Instance.Read(bytes);
        file.Format.Should().BeTheSameInstanceAs(Z80Format.Instance);

        var v2File = file.Should().BeOfType<Z80V2File>().Value;
        AssertMontyV2OrV3<Z80V2File, Z80V2Header>(v2File);
    }

    [Test]
    public void Read_V3()
    {
        using var monty = OpenResource(Resources.AufWiedersehenMontyZ80V3);

        var file = Z80Format.Instance.Read(monty);
        file.Format.Should().BeTheSameInstanceAs(Z80Format.Instance);

        var v3File = file.Should().BeOfType<Z80V3File>().Value;
        AssertMontyV2OrV3<Z80V3File, Z80V3Header>(v3File);
    }

    private static void AssertMontyV1(Z80File file)
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
        file.Registers.Shadow.AF.Should().Equal(0x314);
        file.Registers.Shadow.BC.Should().Equal(0x0345);
        file.Registers.Shadow.DE.Should().Equal(0x023E);
        file.Registers.Shadow.HL.Should().Equal(0x5976);
        file.Header.IFF1.Should().BeFalse();
        file.Header.IFF2.Should().BeFalse();
        file.Header.InterruptMode.Should().Equal(1);
        file.Header.VideoSynchronisation.Should().Equal(VideoSynchronisation.Normal);
        file.Header.Joystick.Should().Equal(Joystick.Cursor);
    }

    private static void AssertMontyV2OrV3<TFile, THeader>(TFile v2OrV3File)
        where TFile : Z80V2OrV3File<THeader>
        where THeader : Z80V2Header
    {
        AssertMontyV1(v2OrV3File);
        v2OrV3File.Header.HardwareMode.Should().Equal(HardwareMode.Spectrum48);
        v2OrV3File.Pages.Should().HaveCount(3);
        v2OrV3File.Pages.Should().OnlyContain(p => p.Header.HardwareMode == HardwareMode.Spectrum48);
        v2OrV3File.Pages[0].Header.PageNumber.Should().Equal(5);
        v2OrV3File.Pages[1].Header.PageNumber.Should().Equal(4);
        v2OrV3File.Pages[2].Header.PageNumber.Should().Equal(8);
    }

    [TestCase(Resources.AufWiedersehenMontyZ80V1Compressed)]
    [TestCase(Resources.AufWiedersehenMontyZ80V1Uncompressed)]
    [TestCase(Resources.AufWiedersehenMontyZ80V2)]
    [TestCase(Resources.AufWiedersehenMontyZ80V3)]
    public void RoundTrip(string resource)
    {
        using var monty = OpenResource(resource);

        var file = Z80Format.Instance.Read(monty);

        var actual = Z80Format.Instance.Write(file);

        monty.Seek(0, SeekOrigin.Begin);
        var expected = monty.ReadAllBytes();

        actual.Should().SequenceEqual(expected);
    }

    [Test]
    public void Read_UnsupportedExtraLength()
    {
        using var stream = new MemoryStream();

        // Write 30 bytes of v1 header with PC = 0 (indicates v2/v3).
        var v1HeaderBytes = new byte[30];
        stream.Write(v1HeaderBytes);

        // Write unsupported extra length (e.g. 99).
        stream.WriteByte(99);
        stream.WriteByte(0);

        // Write enough header bytes to fill 99 extra.
        stream.Write(new byte[99]);

        stream.Position = 0;

        AssertThat.Invoking(() => Z80Format.Instance.Read(stream))
            .Should().Throw<InvalidOperationException>()
            .Exception.Message.Should().Match(".*99.*does not correspond to a known Z80 version.*");
    }

    [Test]
    public void Write_ThrowsForWrongFileType()
    {
        var tapFile = TapFile.CreateCode("test", 0, [0xF3, 0xAF]);

        using var output = new MemoryStream();
        AssertThat.Invoking(() => Z80Format.Instance.Write(tapFile, output))
            .Should().Throw<ArgumentException>();
    }
}