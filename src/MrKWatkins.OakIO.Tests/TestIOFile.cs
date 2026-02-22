namespace MrKWatkins.OakIO.Tests;

internal sealed class TestIOFile : IOFile
{
    private readonly bool canLoad;

    internal TestIOFile(bool canLoad = true)
        : base(TestIOFileFormat.Instance)
    {
        this.canLoad = canLoad;
    }

    public override bool TryLoadInto(Span<byte> memory)
    {
        if (canLoad)
        {
            TestIOFileFormat.Contents.CopyTo(memory);
            return true;
        }

        return false;
    }
}