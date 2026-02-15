namespace MrKWatkins.OakIO;

public abstract class Header : FileComponent
{
    protected Header(int length)
        : base(length)
    {
    }

    protected Header(int length, Stream data)
        : base(length, data)
    {
    }

    protected Header(int length, [InstantHandle] IEnumerable<byte> data)
        : base(length, data)
    {
    }

    protected Header(byte[] data)
        : base(data)
    {
    }
}