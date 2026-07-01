using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Tape;

namespace MrKWatkins.OakIO.Tests.Tapes;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class TapeFormatTests
{
    [Test]
    public void Instance()
    {
        TapeFormat.Instance.Name.Should().Equal("Tape");
        TapeFormat.Instance.FileExtension.Should().Equal("tape");
    }

    [Test]
    public void CanRead() => TapeFormat.Instance.CanRead.Should().BeFalse();

    [Test]
    public void CanWrite() => TapeFormat.Instance.CanWrite.Should().BeFalse();

    [Test]
    public void Read_Throws()
    {
        using var stream = new MemoryStream();
        AssertThat.Invoking(() => TapeFormat.Instance.Read(stream)).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Write_Throws()
    {
        var file = new TapeFile([]);
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        AssertThat.Invoking(() => TapeFormat.Instance.WriteAsync(file, writer)).Should().Throw<NotSupportedException>();
    }
}