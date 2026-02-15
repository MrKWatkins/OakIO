using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.Tests;

internal sealed class TestFileFormat : FileFormat<TestIOFile>
{
    public static readonly TestFileFormat Instance = new();
    public static readonly byte[] Contents = [0x01, 0x02, 0x03, 0x04, 0x05];

    private TestFileFormat()
        : base("Test", "tst")
    {
    }

    public override IOFile Read(Stream stream)
    {
        var contents = stream.ReadAllBytes();
        contents.Should().SequenceEqual(Contents);
        return new TestIOFile();
    }

    protected override void Write(TestIOFile _, Stream stream) => stream.Write(Contents);
}