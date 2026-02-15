namespace MrKWatkins.OakIO.ZXSpectrum.Tap;

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

    public TapHeaderType HeaderType
    {
        get => GetByte<TapHeaderType>(0);
        private init => SetByte(0, value);
    }

    public string Filename
    {
        get => GetString(1, FilenameLength).TrimEnd();
        private init => SetString(1, FilenameLength, value.PadRight(10, ' ').AsSpan()[..10]);
    }

    public ushort DataBlockLength
    {
        get => GetWord(11);
        private init => SetWord(11, value);
    }

    public ushort Parameter1
    {
        get => GetWord(13);
        private init => SetWord(13, value);
    }

    public ushort Parameter2
    {
        get => GetWord(15);
        private init => SetWord(15, value);
    }

    public ushort Location =>
        HeaderType switch
        {
            TapHeaderType.Program => 23755,
            TapHeaderType.Code => Parameter1,
            _ => throw new NotSupportedException($"The {nameof(TapHeaderType)} {HeaderType} is not supported.")
        };

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