namespace MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

internal sealed class Decompressor
{
    private readonly bool endMarker;
    private ReadOnlyMemory<byte> writeBuffer;
    private State state;
    private int repeatCount;
    private byte toRepeat;
    private bool endMarkerReached;

    internal Decompressor(bool endMarker)
    {
        this.endMarker = endMarker;
    }

    internal int Read(Stream stream, Span<byte> buffer)
    {
        if (endMarkerReached)
        {
            return 0;
        }

        var bufferPosition = 0;

        // Do we have a write buffer? If so, complete that first.
        if (writeBuffer.Length > 0)
        {
            ProcessWriteBuffer(buffer, ref bufferPosition);
        }

        // Are we already repeating? If so, complete that first.
        if (state == State.Repeating)
        {
            ProcessRepeat(buffer, ref bufferPosition);
        }

        while (bufferPosition < buffer.Length)
        {
            // Let's get reading.
            var value = stream.ReadByte();
            if (value == -1)
            {
                // End of stream.
                ProcessEndOfStream(buffer, ref bufferPosition);

                // We have read up to the current position.
                return bufferPosition;
            }

            // Process the byte we read.
            switch (state)
            {
                case State.Normal:
                    ProcessNormal(buffer, value, ref bufferPosition);
                    break;

                case State.OneEDRead:
                    ProcessOneEDRead(buffer, value, ref bufferPosition);
                    break;

                case State.TwoEDsRead:
                    ProcessTwoEDsRead(value);
                    break;

                case State.RepeatCountRead:
                    ProcessRepeatCountRead(buffer, value, ref bufferPosition);
                    break;

                case State.OneEndMarkerByteRead:
                    ProcessOneEndMarkerByteRead(buffer, value, ref bufferPosition);
                    break;

                case State.TwoEndMarkerBytesRead:
                    ProcessTwoEndMarkerBytesRead(buffer, value, ref bufferPosition);
                    break;

                case State.ThreeEndMarkerBytesRead:
                    if (ProcessThreeEndMarkerBytesRead(buffer, value, ref bufferPosition))
                    {
                        return bufferPosition;
                    }
                    break;
            }
        }

        return buffer.Length;
    }

    private void ProcessNormal(Span<byte> buffer, int value, ref int bufferPosition)
    {
        switch (value)
        {
            case 0x00 when endMarker:
                state = State.OneEndMarkerByteRead;
                break;

            case 0xED:
                state = State.OneEDRead;
                break;

            default:
                buffer[bufferPosition] = (byte)value;
                bufferPosition++;
                break;
        }
    }

    private void ProcessOneEDRead(Span<byte> buffer, int value, ref int bufferPosition)
    {
        switch (value)
        {
            case 0x00 when endMarker:
                // If one ED is followed by a non-ED byte xx, then we output ED xx. However, if the byte is 00, we might
                // be starting the end marker.
                buffer[bufferPosition] = 0xED;
                bufferPosition++;
                state = State.OneEndMarkerByteRead;
                break;

            case 0xED:
                state = State.TwoEDsRead;
                break;

            default:
                // If one ED is followed by a non-ED byte xx, then we output ED xx.
                writeBuffer = new byte[] { 0xED, (byte)value };
                state = State.Normal;
                ProcessWriteBuffer(buffer, ref bufferPosition);
                break;
        }
    }

    private void ProcessTwoEDsRead(int value)
    {
        repeatCount = value;
        state = State.RepeatCountRead;
    }

    private void ProcessRepeatCountRead(Span<byte> buffer, int value, ref int bufferPosition)
    {
        if (value == 0xED)
        {
            if (repeatCount < 2)
            {
                throw new InvalidOperationException($"Found invalid data while decompressing; repeated sections of 0xED should have length 2 greater, found {repeatCount}.");
            }
        }
        else if (repeatCount < 5)
        {
            throw new InvalidOperationException($"Found invalid data while decompressing; repeated sections not of 0xED should have length 5 or greater, found {repeatCount}.");
        }

        toRepeat = (byte)value;
        state = State.Repeating;
        ProcessRepeat(buffer, ref bufferPosition);
    }

    private void ProcessOneEndMarkerByteRead(Span<byte> buffer, int value, ref int bufferPosition)
    {
        switch (value)
        {
            case 0xED:
                // We've read 00 ED
                state = State.TwoEndMarkerBytesRead;
                break;

            case 0x00:
                // Value is a 00 - could be the start of an end marker, and the previous
                // 00 was just data. Write the previous 00 and keep the state as one
                // end marker byte read.
                writeBuffer = new byte[] { 0x00 };
                ProcessWriteBuffer(buffer, ref bufferPosition);
                break;

            default:
                // We've already read a 00. Process that and the current value.
                writeBuffer = new byte[] { 0x00, (byte)value };
                ProcessWriteBuffer(buffer, ref bufferPosition);
                state = State.Normal;
                break;
        }
    }

    private void ProcessTwoEndMarkerBytesRead(Span<byte> buffer, int value, ref int bufferPosition)
    {
        if (value == 0xED)
        {
            // We've read 00 ED ED.
            state = State.ThreeEndMarkerBytesRead;
        }
        else
        {
            // We've read 00 ED value. If one ED is followed by a non-ED byte xx, then we output ED xx.
            writeBuffer = new byte[] { 0x00, 0xED, (byte)value };
            state = State.Normal;
            ProcessWriteBuffer(buffer, ref bufferPosition);
        }
    }

    private bool ProcessThreeEndMarkerBytesRead(Span<byte> buffer, int value, ref int bufferPosition)
    {
        if (value == 0x00)
        {
            // We've read 00 ED ED 00. We're done.
            endMarkerReached = true;
            return true;
        }

        // We've read 00 ED ED value. Therefore, we need to write 00 and process the two EDs.
        buffer[bufferPosition] = 0x00;
        bufferPosition++;
        ProcessTwoEDsRead(value);
        return false;
    }

    private void ProcessWriteBuffer(Span<byte> buffer, ref int bufferPosition)
    {
        if (writeBuffer.Length < buffer.Length - bufferPosition)
        {
            // Buffer is large enough to complete our write buffer.
            buffer = buffer.Slice(bufferPosition, writeBuffer.Length);
            writeBuffer.Span.CopyTo(buffer);

            bufferPosition += writeBuffer.Length;
            writeBuffer = default;
        }
        else
        {
            // Buffer is not large enough.
            buffer = buffer[bufferPosition..];
            writeBuffer.Span[..buffer.Length].CopyTo(buffer);

            bufferPosition += buffer.Length;
            writeBuffer = writeBuffer[buffer.Length..];
        }
        // No need to update the state as the caller will have set the next state before calling.
    }

    private void ProcessRepeat(Span<byte> buffer, ref int bufferPosition)
    {
        if (repeatCount < buffer.Length - bufferPosition)
        {
            // Buffer is large enough to complete our repeat. State will return to normal after.
            // No need to update the repeatCount as it will just be overwritten next repeat.
            buffer = buffer.Slice(bufferPosition, repeatCount);
            state = State.Normal;
        }
        else
        {
            // Buffer is not large enough.
            buffer = buffer[bufferPosition..];
            repeatCount -= buffer.Length;
        }
        buffer.Fill(toRepeat);
        bufferPosition += buffer.Length;
    }

    private void ProcessEndOfStream(Span<byte> buffer, ref int bufferPosition)
    {
        // If we've read one ED, and we do not have an end marker, then we need to write that ED and finish.
        if (!endMarker && state == State.OneEDRead)
        {
            buffer[bufferPosition] = 0xED;
            bufferPosition++;
            state = State.Normal;
            return;
        }

        // If we're not in a normal state, then something has gone wrong.
        if (state != State.Normal)
        {
            throw new InvalidOperationException("Found truncated data while decompressing; data finished in the middle of a encoding section.");
        }

        // Have we received an expected end marker?
        if (endMarker && !endMarkerReached)
        {
            throw new InvalidOperationException("Found truncated data while decompressing; missing end marker.");
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private enum State
    {
        Normal,
        OneEDRead,
        TwoEDsRead,
        RepeatCountRead,
        Repeating,
        OneEndMarkerByteRead,
        TwoEndMarkerBytesRead,
        ThreeEndMarkerBytesRead
    }
}