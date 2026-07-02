using MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Recording.Rzx;

public sealed class RzxFileTests
{
    [Test]
    public void Constructor_DefaultHeader()
    {
        var file = new RzxFile([new RzxCreatorBlock("OakEmu", 1, 0), new RzxInputRecordingBlock([new RzxInputFrame(1)])]);

        file.Header.Signature.Should().Equal("RZX!");
        file.Header.MinorVersion.Should().Equal((byte)13);
        file.Format.Should().BeOfType<RzxFormat>();
    }

    [Test]
    public void Constructor_ProvidedHeader()
    {
        var header = new RzxHeader(0x42);
        var file = new RzxFile([new RzxCreatorBlock("OakEmu", 1, 0)], header);

        file.Header.Should().BeTheSameInstanceAs(header);
    }

    [Test]
    public void Constructor_NullBlocks_Throws()
    {
        AssertThat.Invoking(() => _ = new RzxFile(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Creator()
    {
        var creator = new RzxCreatorBlock("OakEmu", 1, 0);
        var file = new RzxFile([creator, new RzxInputRecordingBlock([new RzxInputFrame(1)])]);

        file.Creator.Should().BeTheSameInstanceAs(creator);
    }

    [Test]
    public void Creator_Missing_Throws()
    {
        var file = new RzxFile([new RzxInputRecordingBlock([new RzxInputFrame(1)])]);

        AssertThat.Invoking(() => _ = file.Creator).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Snapshot()
    {
        var snapshot = new RzxSnapshotBlock("Z80", [1, 2]);
        var file = new RzxFile([new RzxCreatorBlock("OakEmu", 1, 0), snapshot, new RzxInputRecordingBlock([new RzxInputFrame(1)])]);

        file.Snapshot.Should().BeTheSameInstanceAs(snapshot);
    }

    [Test]
    public void Snapshot_None()
    {
        var file = new RzxFile([new RzxCreatorBlock("OakEmu", 1, 0), new RzxInputRecordingBlock([new RzxInputFrame(1)])]);

        file.Snapshot.Should().BeNull();
    }

    [Test]
    public void InputRecordings()
    {
        var recording = new RzxInputRecordingBlock([new RzxInputFrame(1)]);
        var file = new RzxFile([new RzxCreatorBlock("OakEmu", 1, 0), recording]);

        file.InputRecordings.Should().HaveCount(1);
        file.InputRecordings[0].Should().BeTheSameInstanceAs(recording);
    }

    [Test]
    public void InputRecordings_Multiple()
    {
        var recording1 = new RzxInputRecordingBlock([new RzxInputFrame(1)]);
        var recording2 = new RzxInputRecordingBlock([new RzxInputFrame(2)]);
        var file = new RzxFile([new RzxCreatorBlock("OakEmu", 1, 0), recording1, new RzxSnapshotBlock("Z80", [1]), recording2]);

        file.InputRecordings.Should().HaveCount(2);
        file.InputRecordings[0].Should().BeTheSameInstanceAs(recording1);
        file.InputRecordings[1].Should().BeTheSameInstanceAs(recording2);
    }
}