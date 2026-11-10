using MrKWatkins.OakIO.Resources.Aky;

namespace MrKWatkins.OakIO.Tests.Resources.Aky;

public sealed class AkyTrackTests
{
    [Test]
    public void Blocks_ZeroDuration()
    {
        var file = new AkyFile(AkyTestData.Create(new byte[256], 256));
        file.Patterns[0].Tracks[0].Blocks[0].Duration.Should().Equal(256);
        file.DecodeFrames().Count().Should().Equal(256);
    }

    [Test]
    public void Blocks()
    {
        var file = new AkyFile(AkyTestData.Create([0, 0], 2));
        var track = file.Patterns[0].Tracks[0];
        track.Blocks[0].Duration.Should().Equal(2);
        track.Blocks.Count.Should().Equal(1);
        track.Blocks[0].Offset.Should().Equal(track.Offset);
        track.Blocks[0].RegisterBlockOffset.Should().Equal(AkyTestData.BlockOffset());
    }
}