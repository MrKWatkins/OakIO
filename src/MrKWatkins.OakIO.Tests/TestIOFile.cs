namespace MrKWatkins.OakIO.Tests;

internal sealed class TestIOFile : IOFile
{
    private readonly bool canLoad;

    internal TestIOFile(bool canLoad = true)
        : base(TestFileFormat.Instance)
    {
        this.canLoad = canLoad;
    }

    public override bool TryLoadInto(Span<byte> memory)
    {
        if (canLoad)
        {
            TestFileFormat.Contents.CopyTo(memory);
            return true;
        }

        return false;
    }
}