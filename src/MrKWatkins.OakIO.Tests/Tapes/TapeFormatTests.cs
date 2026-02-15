using MrKWatkins.OakIO.Tape;

namespace MrKWatkins.OakIO.Tests.Tapes;

public sealed class TapeFormatTests
{
    [Test]
    public void Instance()
    {
        TapeFormat.Instance.Name.Should().Equal("Tape");
        TapeFormat.Instance.FileExtension.Should().Equal("tape");
    }

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
        AssertThat.Invoking(() => TapeFormat.Instance.Write(file, stream)).Should().Throw<NotSupportedException>();
    }
}
