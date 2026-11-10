using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Aky;

namespace MrKWatkins.OakIO.Tests.Resources.Aky;

public sealed class AkyFormatTests
{
    [Test]
    public void Instance()
    {
        AkyFormat.Instance.Name.Should().Equal("Arkos AKY Music");
        AkyFormat.Instance.FileExtension.Should().Equal("aky");
        AkyFormat.Instance.FileType.Should().Equal(typeof(AkyFile));
        AkyFormat.Instance.Origin.Should().Equal((ushort)0);
    }

    [TestCase(true, 0)]
    [TestCase(false, 0)]
    [TestCase(true, 32768)]
    [TestCase(false, 32768)]
    public void Read_ByteArray(bool littleEndian, int origin)
    {
        var bytes = AkyTestData.Create([0x39, 0x34, 0x02], littleEndian: littleEndian, origin: (ushort)origin);
        var format = new AkyFormat((ushort)origin);
        var file = format.Read(bytes);
        file.ToByteArray().Should().SequenceEqual(bytes);
        file.Header.IsLittleEndian.Should().Equal(littleEndian);
        file.GetLoopPatternIndex().Should().Equal(0);
        file.Patterns[0].Tracks[0].Blocks[0].RegisterBlockOffset.Should().Equal(AkyTestData.BlockOffset());
    }

    [Test]
    public async Task ReadAsync_Stream()
    {
        var bytes = AkyTestData.Create();
        using var input = new MemoryStream(bytes);
        MusicFormat<AkyFile> typed = AkyFormat.Instance;
        var file = await typed.ReadAsync(input, CancellationToken.None);
        using var output = new MemoryStream();
        await file.WriteAsync(output, CancellationToken.None);
        output.ToArray().Should().SequenceEqual(bytes);
        MusicFormat format = typed;
        format.Read(bytes).Should().BeOfType<AkyFile>();
        using var asyncAgain = new MemoryStream(bytes);
        (await format.ReadAsync(asyncAgain, CancellationToken.None)).ToByteArray().Should().SequenceEqual(bytes);
        ((MusicFile)file).Format.Should().BeTheSameInstanceAs(format);
    }

    [Test]
    public void Read_Stream()
    {
        var bytes = AkyTestData.Create();
        MusicFormat format = AkyFormat.Instance;
        using var input = new MemoryStream(bytes);
        format.Read(input).ToByteArray().Should().SequenceEqual(bytes);
        MusicFormat<AkyFile> typed = AkyFormat.Instance;
        using var again = new MemoryStream(bytes);
        typed.Read(again).ToByteArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public void Read_Discovery()
    {
        using var stream = new MemoryStream(AkyTestData.Create());
        IOFileFormat.Load("music.aky", stream, ResourceFileFormats.AllFormats).Should().BeOfType<AkyFile>();
    }

    [Test]
    public async Task ReadAsync_Cancelled()
    {
        using var stream = new MemoryStream(AkyTestData.Create());
        await stream.Awaiting(input => AkyFormat.Instance.ReadAsync(input, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task WriteAsync_WrongOrigin()
    {
        var file = AkyFormat.Instance.Read(AkyTestData.Create());
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        IOFileFormat format = new AkyFormat(32768);
        await file.Awaiting(value => format.WriteAsync(value, writer).AsTask()).Should().ThrowAsync<ArgumentException>();
    }
}