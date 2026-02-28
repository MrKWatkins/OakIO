using System.Runtime.CompilerServices;
using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO;

/// <summary>
/// Base class for a component of a file, providing access to the underlying binary data.
/// </summary>
/// <param name="data">The raw byte data for this component.</param>
public abstract class IOFileComponent(byte[] data)
{
    /// <summary>
    /// Initialises a new instance of the <see cref="IOFileComponent" /> class with zero-filled data of the specified length.
    /// </summary>
    /// <param name="length">The length of the data in bytes.</param>
    protected IOFileComponent(int length)
        : this(CreateHeader(length))
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="IOFileComponent" /> class by reading data from a stream.
    /// </summary>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="data">The stream to read the data from.</param>
    protected IOFileComponent(int length, Stream data)
        : this(CreateHeader(length, data))
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="IOFileComponent" /> class from a sequence of bytes.
    /// </summary>
    /// <param name="length">The expected length of the data in bytes.</param>
    /// <param name="data">The bytes for this component.</param>
    protected IOFileComponent(int length, [InstantHandle] IEnumerable<byte> data)
        : this(CreateHeader(length, data))
    {
    }

    /// <summary>
    /// Gets the raw byte data for this component.
    /// </summary>
    public IReadOnlyList<byte> Data => data;

    /// <summary>
    /// Gets the length of the data in bytes.
    /// </summary>
    public int Length => data.Length;

    /// <summary>
    /// Returns the data as a writable span.
    /// </summary>
    /// <returns>A span over the data.</returns>
    protected Span<byte> AsSpan() => data;

    /// <summary>
    /// Returns the data as a writable span starting at the specified index.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <returns>A span over the data from the specified index.</returns>
    protected Span<byte> AsSpan(int start) => data.AsSpan(start);

    /// <summary>
    /// Returns the data as a writable span starting at the specified index.
    /// </summary>
    /// <param name="startIndex">The start index.</param>
    /// <returns>A span over the data from the specified index.</returns>
    protected Span<byte> AsSpan(Index startIndex) => data.AsSpan(startIndex);

    /// <summary>
    /// Returns the data as a read-only span.
    /// </summary>
    /// <returns>A read-only span over the data.</returns>
    public ReadOnlySpan<byte> AsReadOnlySpan() => data;

    /// <summary>
    /// Returns the data as a read-only span starting at the specified index.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <returns>A read-only span over the data from the specified index.</returns>
    public ReadOnlySpan<byte> AsReadOnlySpan(int start) => data.AsSpan(start);

    /// <summary>
    /// Returns the data as a read-only span starting at the specified index.
    /// </summary>
    /// <param name="startIndex">The start index.</param>
    /// <returns>A read-only span over the data from the specified index.</returns>
    public ReadOnlySpan<byte> AsReadOnlySpan(Index startIndex) => data.AsSpan(startIndex);

    /// <summary>
    /// Writes the data to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    public void Write(Stream stream) => stream.Write(data);

    /// <summary>
    /// Copies the data to the specified memory span.
    /// </summary>
    /// <param name="memory">The destination span.</param>
    public void CopyTo(Span<byte> memory) => data.CopyTo(memory);

    /// <summary>
    /// Copies the data to the specified memory span at the given start position.
    /// </summary>
    /// <param name="memory">The destination span.</param>
    /// <param name="start">The start position in the destination span.</param>
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

    /// <summary>
    /// Gets the byte at the specified index.
    /// </summary>
    /// <param name="index">The index of the byte.</param>
    /// <returns>The byte value.</returns>
    [Pure]
    protected byte GetByte(int index)
    {
        ValidateByteIndexArguments(index);

        return data[index];
    }

    /// <summary>
    /// Sets the byte at the specified index.
    /// </summary>
    /// <param name="index">The index of the byte.</param>
    /// <param name="value">The value to set.</param>
    protected void SetByte(int index, byte value)
    {
        ValidateByteIndexArguments(index);

        data[index] = value;
    }

    /// <summary>
    /// Gets the byte at the specified index as an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="index">The index of the byte.</param>
    /// <returns>The enum value.</returns>
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

    /// <summary>
    /// Sets the byte at the specified index from an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="index">The index of the byte.</param>
    /// <param name="value">The enum value to set.</param>
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

    /// <summary>
    /// Gets a 16-bit unsigned integer at the specified index.
    /// </summary>
    /// <param name="index">The index of the word.</param>
    /// <param name="endian">The byte order.</param>
    /// <returns>The word value.</returns>
    [Pure]
    protected ushort GetUInt16(int index, Endian endian = Endian.Little)
    {
        ValidateWordIndexArguments(index);

        return data.GetUInt16(index, endian);
    }

    /// <summary>
    /// Sets a 16-bit unsigned integer at the specified index.
    /// </summary>
    /// <param name="index">The index of the word.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="endian">The byte order.</param>
    protected void SetUInt16(int index, ushort value, Endian endian = Endian.Little)
    {
        ValidateWordIndexArguments(index);

        data.SetUInt16(index, value, endian);
    }

    /// <summary>
    /// Gets a 24-bit unsigned integer at the specified index.
    /// </summary>
    /// <param name="index">The index of the value.</param>
    /// <param name="endian">The byte order.</param>
    /// <returns>The 24-bit unsigned integer value.</returns>
    [Pure]
    protected UInt24 GetUInt24(int index, Endian endian = Endian.Little)
    {
        ValidateUInt24IndexArguments(index);

        return data.GetUInt24(index, endian);
    }

    /// <summary>
    /// Sets a 24-bit unsigned integer at the specified index.
    /// </summary>
    /// <param name="index">The index of the value.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="endian">The byte order.</param>
    protected void SetUInt24(int index, UInt24 value, Endian endian = Endian.Little)
    {
        ValidateUInt24IndexArguments(index);

        data.SetUInt24(index, value, endian);
    }

    /// <summary>
    /// Gets a 32-bit signed integer at the specified index.
    /// </summary>
    /// <param name="index">The index of the value.</param>
    /// <param name="endian">The byte order.</param>
    /// <returns>The 32-bit signed integer value.</returns>
    [Pure]
    protected int GetInt32(int index, Endian endian = Endian.Little)
    {
        ValidateUInt32IndexArguments(index);

        return data.GetInt32(index, endian);
    }

    /// <summary>
    /// Sets a 32-bit signed integer at the specified index.
    /// </summary>
    /// <param name="index">The index of the value.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="endian">The byte order.</param>
    protected void SetInt32(int index, int value, Endian endian = Endian.Little)
    {
        ValidateUInt32IndexArguments(index);

        data.SetInt32(index, value, endian);
    }

    /// <summary>
    /// Gets a 32-bit unsigned integer at the specified index.
    /// </summary>
    /// <param name="index">The index of the value.</param>
    /// <param name="endian">The byte order.</param>
    /// <returns>The 32-bit unsigned integer value.</returns>
    [Pure]
    protected uint GetUInt32(int index, Endian endian = Endian.Little)
    {
        ValidateUInt32IndexArguments(index);

        return data.GetUInt32(index, endian);
    }

    /// <summary>
    /// Sets a 32-bit unsigned integer at the specified index.
    /// </summary>
    /// <param name="index">The index of the value.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="endian">The byte order.</param>
    protected void SetUInt32(int index, uint value, Endian endian = Endian.Little)
    {
        ValidateUInt32IndexArguments(index);

        data.SetUInt32(index, value, endian);
    }

    /// <summary>
    /// Gets a 64-bit signed integer at the specified index.
    /// </summary>
    /// <param name="index">The index of the value.</param>
    /// <param name="endian">The byte order.</param>
    /// <returns>The 64-bit signed integer value.</returns>
    [Pure]
    protected long GetInt64(int index, Endian endian = Endian.Little)
    {
        ValidateUInt64IndexArguments(index);

        return data.GetInt64(index, endian);
    }

    /// <summary>
    /// Sets a 64-bit signed integer at the specified index.
    /// </summary>
    /// <param name="index">The index of the value.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="endian">The byte order.</param>
    protected void SetInt64(int index, long value, Endian endian = Endian.Little)
    {
        ValidateUInt64IndexArguments(index);

        data.SetInt64(index, value, endian);
    }

    /// <summary>
    /// Gets a 64-bit unsigned integer at the specified index.
    /// </summary>
    /// <param name="index">The index of the value.</param>
    /// <param name="endian">The byte order.</param>
    /// <returns>The 64-bit unsigned integer value.</returns>
    [Pure]
    protected ulong GetUInt64(int index, Endian endian = Endian.Little)
    {
        ValidateUInt64IndexArguments(index);

        return data.GetUInt64(index, endian);
    }

    /// <summary>
    /// Sets a 64-bit unsigned integer at the specified index.
    /// </summary>
    /// <param name="index">The index of the value.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="endian">The byte order.</param>
    protected void SetUInt64(int index, ulong value, Endian endian = Endian.Little)
    {
        ValidateUInt64IndexArguments(index);

        data.SetUInt64(index, value, endian);
    }

    /// <summary>
    /// Gets a single bit at the specified byte and bit index.
    /// </summary>
    /// <param name="index">The index of the byte.</param>
    /// <param name="bitIndex">The index of the bit within the byte (0-7).</param>
    /// <returns><c>true</c> if the bit is set; <c>false</c> otherwise.</returns>
    [Pure]
    protected bool GetBit(int index, int bitIndex)
    {
        ValidateBitIndexArguments(index, bitIndex);

        return (data[index] & (1 << bitIndex)) != 0;
    }

    /// <summary>
    /// Gets a range of bits from the byte at the specified index.
    /// </summary>
    /// <param name="index">The index of the byte.</param>
    /// <param name="startInclusive">The start bit index, inclusive.</param>
    /// <param name="endInclusive">The end bit index, inclusive.</param>
    /// <returns>The extracted bits as a byte.</returns>
    [Pure]
    protected byte GetBits(int index, int startInclusive, int endInclusive) => GetByte(index).GetBits(startInclusive, endInclusive);

    /// <summary>
    /// Gets a range of bits from the byte at the specified index as an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="index">The index of the byte.</param>
    /// <param name="startInclusive">The start bit index, inclusive.</param>
    /// <param name="endInclusive">The end bit index, inclusive.</param>
    /// <returns>The extracted bits as an enum value.</returns>
    [Pure]
    protected TEnum GetBits<TEnum>(int index, int startInclusive, int endInclusive)
        where TEnum : struct, Enum =>
        ToEnum<TEnum>(GetBits(index, startInclusive, endInclusive));

    /// <summary>
    /// Sets a single bit at the specified byte and bit index.
    /// </summary>
    /// <param name="index">The index of the byte.</param>
    /// <param name="bitIndex">The index of the bit within the byte (0-7).</param>
    /// <param name="value">The value to set the bit to.</param>
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

    /// <summary>
    /// Sets a range of bits in the byte at the specified index.
    /// </summary>
    /// <param name="index">The index of the byte.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="startInclusive">The start bit index, inclusive.</param>
    /// <param name="endInclusive">The end bit index, inclusive.</param>
    protected void SetBits(int index, byte value, int startInclusive, int endInclusive) =>
        data[index] = GetByte(index).SetBits(value, startInclusive, endInclusive);

    /// <summary>
    /// Sets a range of bits in the byte at the specified index from an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="index">The index of the byte.</param>
    /// <param name="value">The enum value to set.</param>
    /// <param name="startInclusive">The start bit index, inclusive.</param>
    /// <param name="endInclusive">The end bit index, inclusive.</param>
    protected void SetBits<TEnum>(int index, TEnum value, int startInclusive, int endInclusive)
        where TEnum : struct, Enum =>
        SetBits(index, ToByte(value), startInclusive, endInclusive);

    /// <summary>
    /// Gets a null-terminated ASCII string at the specified index.
    /// </summary>
    /// <param name="index">The index of the first character.</param>
    /// <param name="maximumLength">The maximum length of the string in bytes.</param>
    /// <returns>The string value.</returns>
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

    /// <summary>
    /// Sets a null-terminated ASCII string at the specified index.
    /// </summary>
    /// <param name="index">The index of the first character.</param>
    /// <param name="maximumLength">The maximum length of the string in bytes.</param>
    /// <param name="value">The string value to set.</param>
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