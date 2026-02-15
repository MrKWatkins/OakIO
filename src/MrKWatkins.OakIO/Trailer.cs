namespace MrKWatkins.OakIO;

public abstract class Trailer : FileComponent
{
    protected Trailer(int length) : base(length)
    {
    }

    protected Trailer(int length, Stream data) : base(length, data)
    {
    }

    protected Trailer(int length, IEnumerable<byte> data) : base(length, data)
    {
    }

    protected Trailer(byte[] data) : base(data)
    {
    }
}