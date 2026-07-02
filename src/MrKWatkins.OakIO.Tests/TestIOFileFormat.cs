using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Tests;

internal sealed class TestIOFileFormat : IOFileFormat<TestIOFile>
{
    public static readonly TestIOFileFormat Instance = new();
    public static readonly byte[] Contents = [0x01, 0x02, 0x03, 0x04, 0x05];

    private TestIOFileFormat()
        : base("Test", "tst")
    {
    }

    public override IOFile Read(Stream stream)
    {
        var contents = stream.ReadAllBytes();
        contents.Should().SequenceEqual(Contents);
        return new TestIOFile();
    }

    protected override ValueTask WriteAsync(TestIOFile _, IBinaryWriter writer) => writer.WriteAsync(Contents);
}