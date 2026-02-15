using System.Runtime.CompilerServices;
using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO;

public abstract class FileComponent(byte[] data)
{
    protected FileComponent(int length)
        : this(CreateHeader(length))
    {
    }

    protected FileComponent(int length, Stream data)
        : this(CreateHeader(length, data))
    {
    }

    protected FileComponent(int length, [InstantHandle] IEnumerable<byte> data)
        : this(CreateHeader(length, data))
    {
    }

    public IReadOnlyList<byte> Data => data;

    public int Length => data.Length;

    protected Span<byte> AsSpan() => data;

    protected Span<byte> AsSpan(int start) => data.AsSpan(start);

    protected Span<byte> AsSpan(Index startIndex) => data.AsSpan(startIndex);

    public ReadOnlySpan<byte> AsReadOnlySpan() => data;

    public ReadOnlySpan<byte> AsReadOnlySpan(int start) => data.AsSpan(start);

    public ReadOnlySpan<byte> AsReadOnlySpan(Index startIndex) => data.AsSpan(startIndex);

    public void Write(Stream stream) => stream.Write(data);

    public void CopyTo(Span<byte> memory) => data.CopyTo(memory);

    public void CopyTo(Span<byte> memory, int start) => data.CopyTo(memory, start);

    [Pure]
    private static byte[] CreateHeader(int length) => new byte[ValidateLength(length)];

    [Pure]
    private static byte[] CreateHeader(int length, Stream stream)
    {
        var header = CreateHeader(length);
        for (var f = 0; f < length; f++)
        {
            var @byte = stream.ReadByte();
            if (@byte == -1)
            {
                throw new ArgumentException($"Expected value to have at least {length} bytes, found {f}.", nameof(stream));
            }

            header[f] = (byte)@byte;
        }

        return header;
    }

    [Pure]
    private static byte[] CreateHeader(int length, [InstantHandle] IEnumerable<byte> bytes)
    {
        var header = bytes.ToArray();
        return header.Length == ValidateLength(length) ? header : throw new ArgumentException($"Expected value to have {length} bytes, found {header.Length}.", nameof(bytes));
    }

    [Pure]
    private static int ValidateLength(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        return length;
    }

    [Pure]
    protected byte GetByte(int index)
    {
        ValidateByteIndexArguments(index);

        return data[index];
    }

    protected void SetByte(int index, byte value)
    {
        ValidateByteIndexArguments(index);

        data[index] = value;
    }

    [Pure]
    protected TEnum GetByte<TEnum>(int index)
        where TEnum : struct, Enum
    {
        ValidateByteIndexArguments(index);

        return ToEnum<TEnum>(data[index]);
    }

    [Pure]
    private static TEnum ToEnum<TEnum>(byte value)
        where TEnum : struct, Enum =>
        // GetEnumUnderlyingType is marked with [Intrinsic] so hopefully the JIT will optimise this case.
        typeof(TEnum).GetEnumUnderlyingType() == typeof(byte)
            ? Unsafe.As<byte, TEnum>(ref value)
            : (TEnum)Enum.ToObject(typeof(TEnum), value);

    protected void SetByte<TEnum>(int index, TEnum value)
        where TEnum : struct, Enum
    {
        ValidateByteIndexArguments(index);

        data[index] = ToByte(value);
    }

    [Pure]
    private static byte ToByte<TEnum>(TEnum value)
        where TEnum : struct, Enum =>
        // GetEnumUnderlyingType is marked with [Intrinsic] so hopefully the JIT will optimise this case.
        typeof(TEnum).GetEnumUnderlyingType() == typeof(byte)
            ? Unsafe.As<TEnum, byte>(ref value)
            : Convert.ToByte(value, null);

    [Pure]
    protected ushort GetWord(int index, Endian endian = Endian.Little)
    {
        ValidateWordIndexArguments(index);

        return data.GetWord(index, endian);
    }

    protected void SetWord(int index, ushort value, Endian endian = Endian.Little)
    {
        ValidateWordIndexArguments(index);

        data.SetWord(index, value, endian);
    }

    [Pure]
    protected int GetUInt24(int index, Endian endian = Endian.Little)
    {
        ValidateUInt24IndexArguments(index);

        return data.GetUInt24(index, endian);
    }

    protected void SetUInt24(int index, int value, Endian endian = Endian.Little)
    {
        ValidateUInt24IndexArguments(index);

        data.SetUInt24(index, value, endian);
    }

    [Pure]
    protected int GetInt32(int index, Endian endian = Endian.Little)
    {
        ValidateUInt32IndexArguments(index);

        return data.GetInt32(index, endian);
    }

    protected void SetInt32(int index, int value, Endian endian = Endian.Little)
    {
        ValidateUInt32IndexArguments(index);

        data.SetInt32(index, value, endian);
    }

    [Pure]
    protected uint GetUInt32(int index, Endian endian = Endian.Little)
    {
        ValidateUInt32IndexArguments(index);

        return data.GetUInt32(index, endian);
    }

    protected void SetUInt32(int index, uint value, Endian endian = Endian.Little)
    {
        ValidateUInt32IndexArguments(index);

        data.SetUInt32(index, value, endian);
    }

    [Pure]
    protected long GetInt64(int index, Endian endian = Endian.Little)
    {
        ValidateUInt64IndexArguments(index);

        return data.GetInt64(index, endian);
    }

    protected void SetInt64(int index, long value, Endian endian = Endian.Little)
    {
        ValidateUInt64IndexArguments(index);

        data.SetInt64(index, value, endian);
    }

    [Pure]
    protected ulong GetUInt64(int index, Endian endian = Endian.Little)
    {
        ValidateUInt64IndexArguments(index);

        return data.GetUInt64(index, endian);
    }

    protected void SetUInt64(int index, ulong value, Endian endian = Endian.Little)
    {
        ValidateUInt64IndexArguments(index);

        data.SetUInt64(index, value, endian);
    }

    [Pure]
    protected bool GetBit(int index, int bitIndex)
    {
        ValidateBitIndexArguments(index, bitIndex);

        return (data[index] & (1 << bitIndex)) != 0;
    }

    [Pure]
    protected byte GetBits(int index, int startInclusive, int endInclusive) => GetByte(index).GetBits(startInclusive, endInclusive);

    [Pure]
    protected TEnum GetBits<TEnum>(int index, int startInclusive, int endInclusive)
        where TEnum : struct, Enum =>
        ToEnum<TEnum>(GetBits(index, startInclusive, endInclusive));

    protected void SetBit(int index, int bitIndex, bool value)
    {
        ValidateBitIndexArguments(index, bitIndex);

        int currentValue = data[index];
        var mask = 1 << bitIndex;
        if (value)
        {
            currentValue |= mask;
        }
        else
        {
            currentValue &= ~mask;
        }

        data[index] = (byte)currentValue;
    }

    protected void SetBits(int index, byte value, int startInclusive, int endInclusive) =>
        data[index] = GetByte(index).SetBits(value, startInclusive, endInclusive);

    protected void SetBits<TEnum>(int index, TEnum value, int startInclusive, int endInclusive)
        where TEnum : struct, Enum =>
        SetBits(index, ToByte(value), startInclusive, endInclusive);

    [Pure]
    protected string GetString(int index, int maximumLength)
    {
        ValidateStringIndexArguments(index, maximumLength);

        var length = 0;
        while (length < maximumLength)
        {
            if (data[index + length] == 0)
            {
                break;
            }

            length++;
        }

        return string.Create(length, data.AsMemory(index, length), (target, source) =>
        {
            var sourceSpan = source.Span;
            for (var f = 0; f < target.Length; f++)
            {
                target[f] = (char)sourceSpan[f];
            }
        });
    }

    protected void SetString(int index, int maximumLength, ReadOnlySpan<char> value)
    {
        ValidateStringIndexArguments(index, maximumLength);
        if (value.Length > maximumLength)
        {
            throw new ArgumentException($"Value is longer ({value.Length}) than {nameof(maximumLength)}. ({maximumLength})", nameof(value));
        }

        var stringIndex = 0;
        while (stringIndex < value.Length)
        {
            var character = value[stringIndex];
            if (!char.IsAscii(character))
            {
                throw new ArgumentException($"Character at index {stringIndex} ('{character}') is not ASCII.", nameof(value));
            }

            data[index + stringIndex] = (byte)character;
            stringIndex++;
        }

        while (stringIndex < maximumLength)
        {
            data[index + stringIndex] = 0;
            stringIndex++;
        }
    }

    private void ValidateByteIndexArguments(int index)
    {
        if (index < 0 || index >= Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Value must be greater than or equal to 0 and less than {nameof(Length)}. ({Length})");
        }
    }

    private void ValidateWordIndexArguments(int index)
    {
        if (index < 0 || index >= Length - 1)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Value must be greater than or equal to 0 and less than {nameof(Length)} - 1. ({Length - 1})");
        }
    }

    private void ValidateUInt24IndexArguments(int index)
    {
        if (index < 0 || index >= Length - 2)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Value must be greater than or equal to 0 and less than {nameof(Length)} - 2. ({Length - 2})");
        }
    }

    private void ValidateUInt32IndexArguments(int index)
    {
        if (index < 0 || index >= Length - 3)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Value must be greater than or equal to 0 and less than {nameof(Length)} - 3. ({Length - 3})");
        }
    }

    private void ValidateUInt64IndexArguments(int index)
    {
        if (index < 0 || index >= Length - 7)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Value must be greater than or equal to 0 and less than {nameof(Length)} - 7. ({Length - 7})");
        }
    }

    private void ValidateBitIndexArguments(int index, int bitIndex)
    {
        ValidateByteIndexArguments(index);

        if (bitIndex is < 0 or > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "Value must be in the range 0 -> 7.");
        }
    }

    private void ValidateStringIndexArguments(int index, int maximumLength)
    {
        ValidateByteIndexArguments(index);

        if (maximumLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumLength), maximumLength, "Value must be greater than 0.");
        }

        var endIndex = index + maximumLength;

        if (endIndex > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumLength), maximumLength, $"{nameof(index)} + {nameof(maximumLength)} ({endIndex}) must be less than or equal to {nameof(Length)}. ({Length})");
        }
    }
}