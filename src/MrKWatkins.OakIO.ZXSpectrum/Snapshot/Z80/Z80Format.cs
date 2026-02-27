using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// File format for Z80 snapshot files.
/// </summary>
// https://worldofspectrum.org/faq/reference/z80format.htm
public sealed class Z80Format : ZXSpectrumSnapshotFormat<Z80File>
{
    /// <summary>
    /// The singleton instance of the Z80 format.
    /// </summary>
    public static readonly Z80Format Instance = new();

    private Z80Format()
        : base("Z80 Snapshot", "z80")
    {
    }

    /// <inheritdoc />
    [Pure]
    protected override IEnumerable<IOFileConverter> CreateConverters()
    {
        yield return new Z80ToSnaConverter();
    }

    /// <inheritdoc />
    protected override Z80File ReadSnapshot(Stream stream)
    {
        var v1HeaderBytes = new byte[30];
        stream.ReadExactly(v1HeaderBytes, 0, 30);
        return v1HeaderBytes.GetWord(6) != 0
            ? ReadV1(stream, v1HeaderBytes)
            : ReadV2OrV3(stream, v1HeaderBytes);
    }

    [MustUseReturnValue]
    private static Z80V1File ReadV1(Stream stream, byte[] v1HeaderBytes)
    {
        var header = new Z80V1Header(v1HeaderBytes);
        var data = stream.ReadAllBytes();

        return new Z80V1File(header, data);
    }

    [MustUseReturnValue]
    private static Z80File ReadV2OrV3(Stream stream, byte[] v1HeaderBytes)
    {
        var extraLength = stream.ReadWordOrThrow();

        // Extra length does not include the 2 bytes for the extraLength word.
        var headerBytes = new byte[30 + 2 + extraLength];
        v1HeaderBytes.CopyTo(headerBytes, 0);
        headerBytes.SetWord(30, extraLength);
        stream.ReadExactly(headerBytes, 32, extraLength);

        switch (extraLength)
        {
            case 23:
                {
                    var header = new Z80V2Header(headerBytes);
                    return new Z80V2File(header, LoadPages(header.HardwareMode, stream));
                }
            case 54 or 55:
                {
                    var header = new Z80V3Header(headerBytes);
                    return new Z80V3File(header, LoadPages(header.HardwareMode, stream));
                }
        }

        throw new InvalidOperationException($"An extra header length of {extraLength} does not correspond to a known Z80 version.");
    }

    [MustUseReturnValue]
    private static IEnumerable<Page> LoadPages(HardwareMode hardwareMode, Stream stream)
    {
        using var peekableStream = new PeekableStream(stream);
        while (!peekableStream.EndOfStream)
        {
            yield return LoadPage(hardwareMode, peekableStream);
        }
    }

    [MustUseReturnValue]
    private static Page LoadPage(HardwareMode hardwareMode, Stream stream)
    {
        var headerBytes = new byte[3];
        stream.ReadExactly(headerBytes);

        var header = new PageHeader(hardwareMode, headerBytes);

        return new Page(header, header.CompressedLength == 0xFFFF ? 16384 : header.CompressedLength, stream);
    }

    /// <inheritdoc />
    protected override void Write(Z80File file, Stream stream)
    {
        if (file is Z80V1File && file.Registers.PC == 0)
        {
            throw new InvalidOperationException("PC cannot be 0 for a v1 file; a PC value of 0 is to specify a v2 or v3 file.");
        }

        stream.Write(file.Header.AsReadOnlySpan());
        if (file is Z80V1File v1File)
        {
            stream.Write(v1File.CompressedData);
        }
        else
        {
            WriteV2OrV3Data((IZ80SnapshotV2OrV3File)file, stream);
        }
    }

    private static void WriteV2OrV3Data(IZ80SnapshotV2OrV3File v2File, Stream stream)
    {
        foreach (var page in v2File.Pages)
        {
            stream.Write(page.Header.AsReadOnlySpan());
            stream.Write(page.AsReadOnlySpan());
        }
    }
}