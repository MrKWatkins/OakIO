namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// A header block in a TAP file describing the following data block.
/// </summary>
public sealed class HeaderBlock : TapBlock<HeaderHeader>
{
    private const int FilenameLength = 10;

    internal HeaderBlock(HeaderHeader header, TapTrailer trailer, byte[] data)
        : base(header, trailer, data)
    {
        if (data.Length != 17)
        {
            throw new ArgumentException("Value must be exactly 17 bytes long.", nameof(data));
        }
    }

    /// <summary>
    /// Creates a new <see cref="HeaderBlock" /> for a code block.
    /// </summary>
    /// <param name="filename">The filename for the block.</param>
    /// <param name="location">The memory location to load the code to.</param>
    /// <param name="length">The length of the data block in bytes.</param>
    /// <returns>A new <see cref="HeaderBlock" /> for a code block.</returns>
    [Pure]
    public static HeaderBlock CreateCode(string filename, ushort location, ushort length)
    {
        var header = new HeaderHeader(19);
        var trailer = new TapTrailer(0);
        var block = new HeaderBlock(header, trailer, new byte[17])
        {
            Filename = filename,
            HeaderType = TapHeaderType.Code,
            DataBlockLength = length,
            Parameter1 = location,
            Parameter2 = 32768
        };

        trailer.Checksum = block.Checksum;
        return block;
    }

    /// <summary>
    /// Creates a new <see cref="HeaderBlock" /> for a BASIC program block.
    /// </summary>
    /// <param name="filename">The filename for the block.</param>
    /// <param name="length">The length of the data block in bytes.</param>
    /// <param name="autostartLineNumber">The autostart line number, or <c>null</c> for no autostart.</param>
    /// <param name="relativeStartOfVariableArea">The relative start of the variable area, or <c>null</c> to default to <paramref name="length" />.</param>
    /// <returns>A new <see cref="HeaderBlock" /> for a BASIC program.</returns>
    [Pure]
    public static HeaderBlock CreateProgram(string filename, ushort length, ushort? autostartLineNumber = null, ushort? relativeStartOfVariableArea = null)
    {
        var header = new HeaderHeader(19);
        var trailer = new TapTrailer(0);
        var block = new HeaderBlock(header, trailer, new byte[17])
        {
            Filename = filename,
            HeaderType = TapHeaderType.Program,
            DataBlockLength = length,
            Parameter1 = autostartLineNumber ?? 65535,
            Parameter2 = relativeStartOfVariableArea ?? length
        };

        trailer.Checksum = block.Checksum;
        return block;
    }

    /// <summary>
    /// Gets the type of the header.
    /// </summary>
    public TapHeaderType HeaderType
    {
        get => GetByte<TapHeaderType>(0);
        private init => SetByte(0, value);
    }

    /// <summary>
    /// Gets the filename stored in the header.
    /// </summary>
    public string Filename
    {
        get => GetString(1, FilenameLength).TrimEnd();
        private init => SetString(1, FilenameLength, value.PadRight(10, ' ').AsSpan()[..10]);
    }

    /// <summary>
    /// Gets the length of the associated data block in bytes.
    /// </summary>
    public ushort DataBlockLength
    {
        get => GetUInt16(11);
        private init => SetUInt16(11, value);
    }

    /// <summary>
    /// Gets the first parameter whose meaning depends on <see cref="HeaderType" />.
    /// </summary>
    public ushort Parameter1
    {
        get => GetUInt16(13);
        private init => SetUInt16(13, value);
    }

    /// <summary>
    /// Gets the second parameter whose meaning depends on <see cref="HeaderType" />.
    /// </summary>
    public ushort Parameter2
    {
        get => GetUInt16(15);
        private init => SetUInt16(15, value);
    }

    /// <summary>
    /// Gets the memory location to load the data to.
    /// </summary>
    [Pure]
    public ushort Location =>
        HeaderType switch
        {
            TapHeaderType.Program => 23755,
            TapHeaderType.Code => Parameter1,
            _ => throw new NotSupportedException($"The {nameof(TapHeaderType)} {HeaderType} is not supported.")
        };

    /// <summary>
    /// Extracts the data from this block as a <see cref="StandardFileHeader" />.
    /// </summary>
    /// <param name="header">The extracted header. Always set for <see cref="HeaderBlock" />.</param>
    /// <returns>Always <c>true</c>.</returns>
    public bool TryGetStandardFileHeader([NotNullWhen(true)] out StandardFileHeader? header)
    {
        header = new StandardFileHeader(HeaderType, Filename, DataBlockLength, Parameter1, Parameter2);
        return true;
    }

    /// <inheritdoc />
    [Pure]
    public override string ToString() =>
        HeaderType switch
        {
            TapHeaderType.Program => $"Program: {Filename}",
            TapHeaderType.NumberArray => $"Number array: {Filename}",
            TapHeaderType.CharacterArray => $"Character array: {Filename}",
            TapHeaderType.Code => $"Bytes: {Filename}",
            _ => $"Invalid: {Filename}"
        };
}