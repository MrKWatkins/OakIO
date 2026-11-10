using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Resources.Aky;

namespace MrKWatkins.OakIO.Tests.Resources.Aky;

public sealed class AkyDecoderTests
{
    [Test]
    public void DecodeFrames_InitialKinds()
    {
        var silent = Frame([0x7C, 7]);
        silent.Registers[7].Should().Equal((byte)55);
        silent.Registers[8].Should().Equal((byte)15);
        silent.Registers[6].Should().Equal((byte)7);
        var software = Frame([0x7D, 3, 0x34, 0x02]);
        software.Registers[0].Should().Equal((byte)0x34);
        software.Registers[1].Should().Equal((byte)2);
        software.Registers[7].Should().Equal((byte)54);
        software.Registers[8].Should().Equal((byte)15);
        var hardware = Frame([0xAE, 5, 0x56, 0x34]);
        hardware.Registers[7].Should().Equal((byte)55);
        hardware.Registers[8].Should().Equal((byte)16);
        hardware.Registers[11].Should().Equal((byte)0x56);
        hardware.Registers[12].Should().Equal((byte)0x34);
        hardware.Registers[13].Should().Equal((byte)10);
        hardware.EnvelopeRetriggers[0].Should().BeTrue();
        var combined = Frame([0xBF, 4, 0x23, 0x01, 0x67, 0x45]);
        combined.Registers[0].Should().Equal((byte)0x23);
        combined.Registers[1].Should().Equal((byte)1);
        combined.Registers[11].Should().Equal((byte)0x67);
        combined.Registers[12].Should().Equal((byte)0x45);
        combined.Registers[7].Should().Equal((byte)54);
    }

    [Test]
    public void DecodeFrames_DifferentialVolumeAndNoise()
    {
        var frames = Frames([0x28, 0x9C, 3, 0], 3);
        frames[0].Registers[8].Should().Equal((byte)5);
        frames[1].Registers[8].Should().Equal((byte)3);
        frames[1].Registers[6].Should().Equal((byte)3);
        frames[1].Registers[7].Should().Equal((byte)55);
        frames[2].Registers[8].Should().Equal((byte)3);
        frames[2].Registers[7].Should().Equal((byte)63);
    }

    [Test]
    public void DecodeFrames_DifferentialSoftware()
    {
        var frames = Frames([0x79, 0x34, 2, 0xCD, 0x56, 0xC3, 9, 0x25], 3);
        frames[1].Registers[0].Should().Equal((byte)0x56);
        frames[1].Registers[1].Should().Equal((byte)3);
        frames[1].Registers[8].Should().Equal((byte)3);
        frames[1].Registers[6].Should().Equal((byte)9);
        frames[1].Registers[7].Should().Equal((byte)54);
        frames[2].Registers[0].Should().Equal((byte)0x56);
        frames[2].Registers[8].Should().Equal((byte)9);
        frames[2].Registers[7].Should().Equal((byte)62);
    }

    [Test]
    public void DecodeFrames_DifferentialHardwareAndRetrigger()
    {
        var frames = Frames([0xA2, 0x34, 0x12, 0xEA, 0x78, 0x56, 0x2F, 0x0A], 3);
        frames[1].Registers[11].Should().Equal((byte)0x78);
        frames[1].Registers[12].Should().Equal((byte)0x56);
        frames[1].Registers[6].Should().Equal((byte)5);
        frames[1].Registers[13].Should().Equal((byte)10);
        frames[1].EnvelopeRetriggers[0].Should().BeTrue();
        frames[2].EnvelopeRetriggers[0].Should().BeFalse();
        frames[2].Registers[7].Should().Equal((byte)63);
    }

    [Test]
    public void DecodeFrames_DifferentialCombined()
    {
        var frames = Frames([0xA3, 0x34, 0x02, 0x78, 0x56, 0xFF, 0x12, 0x34, 0x56, 0x07, 0x0B, 0x3F], 2);
        frames[1].Registers[0].Should().Equal((byte)0x56);
        frames[1].Registers[1].Should().Equal((byte)7);
        frames[1].Registers[11].Should().Equal((byte)0x12);
        frames[1].Registers[12].Should().Equal((byte)0x34);
        frames[1].Registers[13].Should().Equal((byte)11);
        frames[1].Registers[6].Should().Equal((byte)7);
        frames[1].Registers[7].Should().Equal((byte)54);
    }

    [Test]
    public void DecodeFrames_BlockLoop()
    {
        var bytes = AkyTestData.Create([0x79, 0x34, 2, 0x3D, 8, 0, 0], 4);
        bytes.SetUInt16(AkyTestData.BlockOffset() + 5, (ushort)(AkyTestData.BlockOffset() + 3));
        var frames = new AkyFile(bytes).DecodeFrames().ToArray();
        frames.Length.Should().Equal(4);
        frames.Select(frame => frame.Registers[8]).Should().SequenceEqual(15, 15, 15, 15);
        frames[0].Registers.Should().NotBeTheSameInstanceAs(frames[1].Registers);
    }

    [Test]
    public void DecodeFrames_InvalidBlockLoop()
    {
        var bytes = AkyTestData.Create([0, 8, 0, 0], 2);
        bytes.SetUInt16(AkyTestData.BlockOffset() + 2, (ushort)(AkyTestData.BlockOffset() + 1));
        AssertThat.Invoking(() => new AkyFile(bytes).DecodeFrames().ToArray()).Should().Throw<InvalidDataException>();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void DecodeFrames_SidCpc(bool littleEndian)
    {
        var frames = SidFrames([0xAC, 2, 255, 0x39, 0x12, 0x03, 0x25, 16, 7, 254, 3, 0x19, 16, 0, 0], 4, AkySidProfile.Cpc, littleEndian);
        var initial = (AkyCpcSidState)frames[0].SidStates[0]!;
        initial.Profile.Should().Equal(AkySidProfile.Cpc);
        initial.LowShelfVolume.Should().Equal((byte)11);
        initial.HighShelfDuration.Should().Equal((byte)2);
        initial.LowShelfDuration.Should().Equal((byte)255);
        frames[0].Registers[8].Should().Equal((byte)7);
        frames[0].Registers[0].Should().Equal((byte)(littleEndian ? 0x12 : 3));
        frames[1].SidStates[0].Should().BeTheSameInstanceAs(initial);
        frames[1].Registers[8].Should().Equal((byte)9);
        var updated = (AkyCpcSidState)frames[2].SidStates[0]!;
        updated.LowShelfVolume.Should().Equal((byte)3);
        updated.HighShelfDuration.Should().Equal((byte)254);
        updated.LowShelfDuration.Should().Equal((byte)3);
        frames[2].Registers[8].Should().Equal((byte)6);
        frames[3].SidStates[0].Should().BeNull();
        frames[3].Registers[7].Should().Equal((byte)63);
        frames[0].SidStates.Skip(1).Should().SequenceEqual(new AkySidState?[2]);
        frames[0].SidStates.Should().NotBeTheSameInstanceAs(frames[1].SidStates);
        initial.LowShelfVolume.Should().Equal((byte)11);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void DecodeFrames_SidAmstradPlus(bool littleEndian)
    {
        var frames = SidFrames([0xBC, 0x34, 0x02, 0x78, 0x0F, 0xA2, 0x56, 0x01, 16, 1, 0, 0, 255, 255, 0x0A], 2, AkySidProfile.AmstradPlus, littleEndian);
        var initial = (AkyAmstradPlusSidState)frames[0].SidStates[0]!;
        initial.Profile.Should().Equal(AkySidProfile.AmstradPlus);
        initial.LowShelfVolume.Should().Equal((byte)15);
        initial.HighShelfDuration.Should().Equal((ushort)(littleEndian ? 0x234 : 0x3402));
        initial.LowShelfDuration.Should().Equal((ushort)(littleEndian ? 0xF78 : 0x780F));
        frames[0].Registers[8].Should().Equal((byte)16);
        frames[0].EnvelopeRetriggers[0].Should().BeTrue();
        var updated = (AkyAmstradPlusSidState)frames[1].SidStates[0]!;
        updated.LowShelfVolume.Should().Equal((byte)0);
        updated.HighShelfDuration.Should().Equal((ushort)0);
        updated.LowShelfDuration.Should().Equal(ushort.MaxValue);
        frames[1].EnvelopeRetriggers[0].Should().BeFalse();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void DecodeFrames_SidAtariSt(bool littleEndian)
    {
        var frames = SidFrames([0x80, 1, 255, 7, 0, 0, 16, 31, 2, 0x81, 6, 0xFE, 0], 2, AkySidProfile.AtariSt, littleEndian);
        var initial = (AkyAtariStSidState)frames[0].SidStates[0]!;
        initial.Profile.Should().Equal(AkySidProfile.AtariSt);
        initial.LowShelfVolume.Should().Equal((byte)0);
        initial.HighShelfTimerControl.Should().Equal((byte)1);
        initial.HighShelfTimerData.Should().Equal((byte)255);
        initial.LowShelfTimerControl.Should().Equal((byte)7);
        initial.LowShelfTimerData.Should().Equal((byte)0);
        var updated = (AkyAtariStSidState)frames[1].SidStates[0]!;
        updated.LowShelfVolume.Should().Equal((byte)15);
        updated.HighShelfTimerControl.Should().Equal((byte)2);
        updated.HighShelfTimerData.Should().Equal((byte)0x81);
        updated.LowShelfTimerControl.Should().Equal((byte)6);
        updated.LowShelfTimerData.Should().Equal((byte)0xFE);
    }

    [Test]
    public void DecodeFrames_SidChannelsAreIndependent()
    {
        var bytes = AkyTestData.Create([0x84, 3, 4, 0x79, 0x34, 0x02, 16, 0, 0x25], 2, channels: 6);
        var secondBlock = bytes.Length;
        bytes = [.. bytes, 0xA4, 7, 8, 0x79, 0x45, 0x03, 0x25];
        var track = 10 + 2 + 12 + 4;
        bytes.SetUInt16(track + 4 * 3 + 1, (ushort)secondBlock);
        var frames = new AkyFile(bytes).DecodeFrames().ToArray();
        var first = (AkyCpcSidState)frames[0].SidStates[0]!;
        var second = (AkyCpcSidState)frames[0].SidStates[4]!;
        first.LowShelfVolume.Should().Equal((byte)1);
        first.HighShelfDuration.Should().Equal((byte)3);
        second.LowShelfVolume.Should().Equal((byte)9);
        second.HighShelfDuration.Should().Equal((byte)7);
        frames[1].SidStates[0].Should().BeNull();
        frames[1].SidStates[4].Should().BeTheSameInstanceAs(second);
        foreach (var index in new[] { 1, 2, 3, 5 })
        {
            frames[0].SidStates[index].Should().BeNull();
            frames[1].SidStates[index].Should().BeNull();
        }
        frames[1].Registers[8].Should().Equal((byte)9);
        frames[1].Registers[14 + 9].Should().Equal((byte)9);
        frames[0].SidStates[0].Should().BeTheSameInstanceAs(first);
    }

    [Test]
    public void DecodeFrames_SidActivatedAfterInitialState()
    {
        var frames = SidFrames([0, 16, 9, 2, 3, 0x3D], 2, AkySidProfile.Cpc);
        frames[0].SidStates[0].Should().BeNull();
        var sid = (AkyCpcSidState)frames[1].SidStates[0]!;
        sid.LowShelfVolume.Should().Equal((byte)4);
        sid.HighShelfDuration.Should().Equal((byte)2);
        sid.LowShelfDuration.Should().Equal((byte)3);
        frames[1].Registers[8].Should().Equal((byte)15);
    }

    [Test]
    public void DecodeFrames_SidResetsAtNewBlock()
    {
        var bytes = AkyTestData.Create([0x84, 3, 4, 0x28, 0x38], 2);
        var block = AkyTestData.BlockOffset();
        var track = 18;
        bytes[track] = 1;
        bytes[track + 3] = 1;
        bytes.SetUInt16(track + 4, (ushort)(block + 4));
        var frames = new AkyFile(bytes).DecodeFrames().ToArray();
        frames[0].SidStates[0].Should().NotBeNull();
        frames[1].SidStates[0].Should().BeNull();
        frames[1].Registers[8].Should().Equal((byte)7);
    }

    [Test]
    public void DecodeFrames_SidBlockLoop()
    {
        var bytes = AkyTestData.Create([0, 16, 11, 5, 6, 0x3D, 8, 0, 0], 4);
        bytes.SetUInt16(AkyTestData.BlockOffset() + 7, (ushort)(AkyTestData.BlockOffset() + 1));
        var frames = new AkyFile(bytes).DecodeFrames().ToArray();
        frames.Length.Should().Equal(4);
        frames[0].SidStates[0].Should().BeNull();
        foreach (var frame in frames.Skip(1))
        {
            var sid = (AkyCpcSidState)frame.SidStates[0]!;
            sid.LowShelfVolume.Should().Equal((byte)5);
            sid.HighShelfDuration.Should().Equal((byte)5);
            sid.LowShelfDuration.Should().Equal((byte)6);
            frame.Registers[8].Should().Equal((byte)15);
        }
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(4)]
    public void DecodeFrames_PartialChip(int channels)
    {
        var frame = new AkyFile(AkyTestData.Create(channels: channels)).DecodeFrames().Single();
        frame.Registers.Count.Should().Equal((channels + 2) / 3 * 14);
        frame.SidStates.Count.Should().Equal(channels);
        frame.SidStates.Should().SequenceEqual(new AkySidState?[channels]);
        for (var chip = 0; chip < (channels + 2) / 3; ++chip)
        {
            frame.Registers[chip * 14 + 7].Should().Equal((byte)63);
        }
    }

    [TestCase(0x81)]
    [TestCase(0xC0)]
    [TestCase(0xC1)]
    public void DecodeFrames_InvalidInitialPacket(byte code) =>
        AssertThat.Invoking(() => Frame([code])).Should().Throw<InvalidDataException>();

    [TestCase(2)]
    [TestCase(30)]
    [TestCase(32)]
    [TestCase(255)]
    public void DecodeFrames_InvalidSidSettings(byte settings) =>
        AssertThat.Invoking(() => Frames([0, 16, settings], 2)).Should().Throw<InvalidDataException>();

    [TestCase(0x80)]
    [TestCase(0)]
    public void DecodeFrames_SidInVersionZero(byte initial)
    {
        var bytes = AkyTestData.Create(initial == 0 ? [0, 16, 0, 0] : [0x80, 2, 3, 0], 2);
        bytes[0] = 128;
        AssertThat.Invoking(() => new AkyFile(bytes).DecodeFrames().ToArray()).Should().Throw<InvalidDataException>();
    }

    [TestCase(AkySidProfile.Cpc, 0)]
    [TestCase(AkySidProfile.Cpc, 1)]
    [TestCase(AkySidProfile.Cpc, 2)]
    [TestCase(AkySidProfile.AmstradPlus, 0)]
    [TestCase(AkySidProfile.AmstradPlus, 1)]
    [TestCase(AkySidProfile.AmstradPlus, 2)]
    [TestCase(AkySidProfile.AmstradPlus, 3)]
    [TestCase(AkySidProfile.AmstradPlus, 4)]
    [TestCase(AkySidProfile.AtariSt, 0)]
    [TestCase(AkySidProfile.AtariSt, 1)]
    [TestCase(AkySidProfile.AtariSt, 2)]
    [TestCase(AkySidProfile.AtariSt, 3)]
    [TestCase(AkySidProfile.AtariSt, 4)]
    public void DecodeFrames_TruncatedSid(AkySidProfile profile, int bytesAfterPrefix)
    {
        var bytes = AkyTestData.Create([0x80]);
        Array.Resize(ref bytes, AkyTestData.BlockOffset() + 1 + bytesAfterPrefix);
        bytes[0] |= (byte)((int)profile << 4);
        AssertThat.Invoking(() => new AkyFile(bytes).DecodeFrames().ToArray()).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void DecodeFrames_SidControlLoopWithoutFrame()
    {
        var bytes = AkyTestData.Create([0, 16, 0, 8, 0, 0], 2);
        bytes.SetUInt16(AkyTestData.BlockOffset() + 4, (ushort)(AkyTestData.BlockOffset() + 1));
        AssertThat.Invoking(() => new AkyFile(bytes).DecodeFrames().ToArray()).Should().Throw<InvalidDataException>();
    }

    [TestCase(0)]
    [TestCase(32)]
    public void DecodeFrames_InvalidNoise(byte value) =>
        AssertThat.Invoking(() => Frame([4, value])).Should().Throw<InvalidDataException>();

    [Test]
    public void DecodeFrames_TruncatedPacket()
    {
        var bytes = AkyTestData.Create([1]);
        AssertThat.Invoking(() => new AkyFile(bytes).DecodeFrames().ToArray()).Should().Throw<InvalidDataException>();
    }

    [Pure]
    private static AkyFrame[] SidFrames(byte[] packets, int duration, AkySidProfile profile, bool littleEndian = true)
    {
        var bytes = AkyTestData.Create(packets, duration, littleEndian: littleEndian, origin: 0x4000);
        bytes[0] |= (byte)((int)profile << 4);
        var file = new AkyFile(bytes, new AkyFormat(0x4000));
        file.ToByteArray().Should().SequenceEqual(bytes);
        return file.DecodeFrames().ToArray();
    }

    [Pure]
    private static AkyFrame Frame(byte[] bytes) => Frames(bytes, 1)[0];

    [Pure]
    private static AkyFrame[] Frames(byte[] bytes, int duration) => new AkyFile(AkyTestData.Create(bytes, duration)).DecodeFrames().ToArray();
}