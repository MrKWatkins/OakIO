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

    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader)
    {
        var contents = await reader.ReadToEndAsync();
        contents.Should().SequenceEqual(Contents);
        return new TestIOFile();
    }

    protected override ValueTask WriteAsync(TestIOFile _, IBinaryWriter writer) => writer.WriteAsync(Contents);
}