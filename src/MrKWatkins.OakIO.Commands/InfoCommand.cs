using System.Text.Json;
using MrKWatkins.OakIO.Commands.FileInfo;
using MrKWatkins.OakIO.ZXSpectrum;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Tape;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using Pzx = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;
using TzxTape = MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

namespace MrKWatkins.OakIO.Commands;

public sealed class InfoCommand
{
    [Pure]
    public static FileInfoResult GetFileInfo(string inputFilename, byte[] inputData)
    {
        using var inputStream = new MemoryStream(inputData);
        return GetFileInfo(inputFilename, inputStream);
    }

    [Pure]
    public static FileInfoResult GetFileInfo(string inputFilename, Stream inputStream)
    {
        var file = ZXSpectrumFile.Read(inputFilename, inputStream);
        return BuildFileInfo(file);
    }

    [Pure]
    public static string GetFileInfoJson(string inputFilename, byte[] inputData)
    {
        var fileInfo = GetFileInfo(inputFilename, inputData);
        return JsonSerializer.Serialize(fileInfo, FileInfoJsonContext.Default.FileInfoResult);
    }

    [Pure]
    public static string Execute(string inputFilename, byte[] inputData, string indent = "    ")
    {
        using var output = new StringWriter();
        var fileInfo = GetFileInfo(inputFilename, inputData);
        WriteFileInfo(output, fileInfo, indent);
        return output.ToString();
    }

    public static void Execute(string inputFilename, Stream inputStream, TextWriter output, string indent = "    ")
    {
        var fileInfo = GetFileInfo(inputFilename, inputStream);
        WriteFileInfo(output, fileInfo, indent);
    }

    [Pure]
    private static FileInfoResult BuildFileInfo(IOFile file)
    {
        var type = file switch
        {
            ZXSpectrumTapeFile => "tape",
            ZXSpectrumSnapshotFile => "snapshot",
            _ => throw new NotSupportedException($"The file type {file.GetType().Name} is not supported.")
        };

        var convertibleTo = IOFileConversion.GetSupportedConversionFormats(file.Format)
            .Where(f => f.CanWrite)
            .Select(f => new ConvertibleFormat(f.Name, f.FileExtension))
            .ToList();

        var sections = file switch
        {
            TapFile tap => tap.ToInfoSections(),
            TzxTape.TzxFile tzx => tzx.ToInfoSections(),
            Pzx.PzxFile pzx => pzx.ToInfoSections(),
            ZXSpectrumSnapshotFile snapshot => snapshot.ToInfoSections(),
            _ => throw new NotSupportedException($"The file type {file.GetType().Name} is not supported.")
        };

        return new FileInfoResult(file.Format.Name, file.Format.FileExtension, type, convertibleTo, sections);
    }

    private static void WriteFileInfo(TextWriter output, FileInfoResult fileInfo, string indent = "    ")
    {
        output.WriteLine($"Format: {fileInfo.Format}");

        foreach (var section in fileInfo.Sections)
        {
            WriteSection(output, section, "", indent);
        }
    }

    private static void WriteSection(TextWriter output, InfoSection section, string currentIndent, string indent = "    ")
    {
        if (section.Properties.Count > 0)
        {
            output.WriteLine($"{currentIndent}{section.Title}:");
            foreach (var prop in section.Properties)
            {
                output.WriteLine($"{currentIndent}{indent}{prop.Name}: {prop.Value}");
            }
        }

        if (section.Items.Count > 0)
        {
            output.WriteLine($"{currentIndent}{section.Title}: {section.Items.Count}");
            foreach (var (item, index) in section.Items.Select((item, i) => (item, i + 1)))
            {
                output.Write($"{currentIndent}{indent}{index}: {item.Title}");

                if (item.Properties.Count > 0)
                {
                    var propSummary = string.Join(", ", item.Properties.Select(p => $"{p.Name}: {p.Value}"));
                    output.Write($" ({propSummary})");
                }

                output.WriteLine();

                if (item.Details.Count > 0)
                {
                    foreach (var (key, value) in item.Details)
                    {
                        output.WriteLine($"{currentIndent}{indent}{indent}{key}: {value}");
                    }
                }

                if (item.Sections.Count > 0)
                {
                    foreach (var nestedSection in item.Sections)
                    {
                        WriteSection(output, nestedSection, currentIndent + indent, indent);
                    }
                }
            }
        }
    }
}