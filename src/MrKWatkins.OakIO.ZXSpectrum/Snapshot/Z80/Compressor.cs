namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// Compresses data using the Z80 snapshot compression format.
/// </summary>
public sealed class Compressor
{
    private readonly bool endMarker;
    private byte? previousByte;
    private int repeatCount;

    internal Compressor(bool endMarker)
    {
        this.endMarker = endMarker;
    }

    internal void Write(Stream stream, ReadOnlySpan<byte> buffer)
    {
        foreach (var currentByte in buffer)
        {
            if (currentByte == previousByte)
            {
                if (repeatCount == 255)
                {
                    WriteRepeat(stream);
                    repeatCount = 1;
                }
                else
                {
                    repeatCount++;
                }
                continue;
            }

            if (previousByte == 0xED && repeatCount == 1)
            {
                // A non-ED byte following a literal ED is not encoded.
                stream.WriteByte(0xED);
                stream.WriteByte(currentByte);
                previousByte = null;
                repeatCount = 0;
                continue;
            }

            if (previousByte != null)
            {
                WriteRepeat(stream);
            }

            previousByte = currentByte;
            repeatCount = 1;
        }
    }

    private void WriteRepeat(Stream stream)
    {
        // Occurs for empty input.
        if (previousByte == null)
        {
            return;
        }

        if (previousByte!.Value == 0xED)
        {
            WriteEDRepeat(stream);
        }
        else
        {
            WriteNormalRepeat(stream, previousByte!.Value);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private void WriteEDRepeat(Stream stream)
    {
        if (repeatCount == 1)
        {
            stream.WriteByte(0xED);
            return;
        }

        stream.WriteByte(0xED);
        stream.WriteByte(0xED);
        stream.WriteByte((byte)repeatCount);
        stream.WriteByte(0xED);
    }

    private void WriteNormalRepeat(Stream stream, byte value)
    {
        if (repeatCount >= 5)
        {
            stream.WriteByte(0xED);
            stream.WriteByte(0xED);
            stream.WriteByte((byte)repeatCount);
            stream.WriteByte(value);
            return;
        }

        for (var f = 0; f < repeatCount; f++)
        {
            stream.WriteByte(value);
        }
    }

    internal void Close(Stream stream)
    {
        WriteRepeat(stream);

        if (endMarker)
        {
            stream.WriteByte(0x00);
            stream.WriteByte(0xED);
            stream.WriteByte(0xED);
            stream.WriteByte(0x00);
        }
    }
}