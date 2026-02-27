using System.Globalization;
using System.Text.Json;
using MrKWatkins.OakIO.ZXSpectrum;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;
using MrKWatkins.OakIO.ZXSpectrum.Tape;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;
using Pzx = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

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
    public static string Execute(string inputFilename, byte[] inputData)
    {
        using var output = new StringWriter();
        var fileInfo = GetFileInfo(inputFilename, inputData);
        WriteFileInfo(output, fileInfo);
        return output.ToString();
    }

    public static void Execute(string inputFilename, Stream inputStream, TextWriter output)
    {
        var fileInfo = GetFileInfo(inputFilename, inputStream);
        WriteFileInfo(output, fileInfo);
    }

    [Pure]
    private static FileInfoResult BuildFileInfo(IOFile file)
    {
        var type = file switch
        {
            ZXSpectrumTapeFile => "tape",
            ZXSpectrumSnapshotFile => "snapshot",
            _ => "unknown"
        };

        var convertibleTo = IOFileConversion.GetSupportedConversionFormats(file.Format)
            .Where(f => f.CanWrite)
            .Select(f => new ConvertibleFormat(f.Name, f.FileExtension))
            .ToList();

        var sections = file switch
        {
            TapFile tap => BuildTapSections(tap),
            TzxFile tzx => BuildTzxSections(tzx),
            Pzx.PzxFile pzx => BuildPzxSections(pzx),
            ZXSpectrumSnapshotFile snapshot => BuildSnapshotSections(snapshot),
            _ => new List<InfoSection>()
        };

        return new FileInfoResult(file.Format.Name, file.Format.FileExtension, type, convertibleTo, sections);
    }

    // TAP

    [Pure]
    private static List<InfoSection> BuildTapSections(TapFile file)
    {
        var items = file.Blocks.Select(BuildTapBlockItem).ToList();
        return [new InfoSection("Blocks", Items: items)];
    }

    [Pure]
    private static InfoItem BuildTapBlockItem(TapBlock block) => block switch
    {
        HeaderBlock header => new InfoItem(
            header.ToString(),
            Properties:
            [
                new InfoProperty("Type", "Header"),
                new InfoProperty("Header Type", header.HeaderType.ToString()),
                new InfoProperty("Filename", header.Filename),
                new InfoProperty("Data Length", header.DataBlockLength.ToString(NumberFormatInfo.InvariantInfo), "decimal"),
                ..(header.HeaderType is TapHeaderType.Program or TapHeaderType.Code
                    ? [new InfoProperty("Location", header.Location.ToString(NumberFormatInfo.InvariantInfo), "decimal")]
                    : Array.Empty<InfoProperty>())
            ]),
        DataBlock data => new InfoItem(
            data.ToString(),
            Properties:
            [
                new InfoProperty("Type", "Data"),
                new InfoProperty("Length", data.Header.BlockLength.ToString(NumberFormatInfo.InvariantInfo), "decimal")
            ]),
        _ => new InfoItem(block.ToString() ?? block.GetType().Name)
    };

    // TZX

    [Pure]
    private static List<InfoSection> BuildTzxSections(TzxFile file)
    {
        var sections = new List<InfoSection>();

        var archiveInfoBlock = file.Blocks.OfType<ArchiveInfoBlock>().FirstOrDefault();
        if (archiveInfoBlock != null)
        {
            var properties = archiveInfoBlock.Entries.Select(e =>
                new InfoProperty(e.Type.ToDescription(), e.Text)
            ).ToList();
            sections.Add(new InfoSection("Archive Info", "file", Properties: properties));
        }

        var blocks = file.Blocks.Where(b => b is not ArchiveInfoBlock).ToList();
        var index = 0;
        var items = BuildTzxBlockItems(blocks, ref index);
        sections.Add(new InfoSection("Blocks", Items: items));

        return sections;
    }

    [Pure]
    private static List<InfoItem> BuildTzxBlockItems(List<TzxBlock> blocks, ref int index)
    {
        var items = new List<InfoItem>();
        while (index < blocks.Count)
        {
            var block = blocks[index];

            if (block is LoopEndBlock or GroupEndBlock)
            {
                return items;
            }

            if (block is LoopStartBlock loopStart)
            {
                index++;
                var body = BuildTzxBlockItems(blocks, ref index);
                items.Add(new InfoItem(
                    "Loop",
                    Properties: [new InfoProperty("Repetitions", loopStart.Header.NumberOfRepetitions.ToString(NumberFormatInfo.InvariantInfo), "decimal")],
                    Sections: [new InfoSection("Blocks", Items: body)]));
            }
            else if (block is GroupStartBlock groupStart)
            {
                index++;
                var body = BuildTzxBlockItems(blocks, ref index);
                items.Add(new InfoItem(
                    $"Group: {groupStart.Text}",
                    Sections: [new InfoSection("Blocks", Items: body)]));
            }
            else
            {
                items.Add(BuildTzxBlockItem(block));
            }

            index++;
        }

        return items;
    }

    [Pure]
    private static InfoItem BuildTzxBlockItem(TzxBlock block)
    {
        switch (block.Header)
        {
            case StandardSpeedDataHeader h:
                return new InfoItem(
                    "Standard Speed Data",
                    Properties:
                    [
                        new InfoProperty("Length", h.BlockLength.ToString(NumberFormatInfo.InvariantInfo), "decimal"),
                        new InfoProperty("Pause After", $"{h.PauseAfterBlockMs} ms")
                    ]);

            case TurboSpeedDataHeader h:
                return new InfoItem(
                    "Turbo Speed Data",
                    Properties:
                    [
                        new InfoProperty("Length", h.BlockLength.ToString(NumberFormatInfo.InvariantInfo), "decimal"),
                        new InfoProperty("Pause After", $"{h.PauseAfterBlockMs} ms")
                    ],
                    Details: new Dictionary<string, string>
                    {
                        ["Pilot Pulse"] = $"{h.TStatesInPilotPulse} T-States",
                        ["Pilot Tone Pulses"] = h.PulsesInPilotTone.ToString(NumberFormatInfo.InvariantInfo),
                        ["Sync First Pulse"] = $"{h.TStatesInSyncFirstPulse} T-States",
                        ["Sync Second Pulse"] = $"{h.TStatesInSyncSecondPulse} T-States",
                        ["Zero Bit Pulse"] = $"{h.TStatesInZeroBitPulse} T-States",
                        ["One Bit Pulse"] = $"{h.TStatesInOneBitPulse} T-States",
                        ["Used Bits in Last Byte"] = h.UsedBitsInLastByte.ToString(NumberFormatInfo.InvariantInfo)
                    });

            case PureDataHeader h:
                return new InfoItem(
                    "Pure Data",
                    Properties:
                    [
                        new InfoProperty("Length", h.BlockLength.ToString(NumberFormatInfo.InvariantInfo), "decimal"),
                        new InfoProperty("Pause After", $"{h.PauseAfterBlockMs} ms")
                    ],
                    Details: new Dictionary<string, string>
                    {
                        ["Zero Bit Pulse"] = $"{h.TStatesInZeroBitPulse} T-States",
                        ["One Bit Pulse"] = $"{h.TStatesInOneBitPulse} T-States",
                        ["Used Bits in Last Byte"] = h.UsedBitsInLastByte.ToString(NumberFormatInfo.InvariantInfo)
                    });

            case PureToneHeader h:
                return new InfoItem(
                    "Pure Tone",
                    Properties:
                    [
                        new InfoProperty("Pulse Count", h.NumberOfPulses.ToString(NumberFormatInfo.InvariantInfo), "decimal"),
                        new InfoProperty("Pulse Length", $"{h.LengthOfPulse} T-States")
                    ]);

            case PulseSequenceHeader h:
                return new InfoItem(
                    "Pulse Sequence",
                    Properties: [new InfoProperty("Pulse Count", h.NumberOfPulses.ToString(NumberFormatInfo.InvariantInfo), "decimal")]);

            case PauseHeader h:
                return new InfoItem(
                    "Pause",
                    Properties: [new InfoProperty("Duration", $"{h.PauseMs} ms")]);

            case TextDescriptionHeader when block is TextDescriptionBlock textBlock:
                return new InfoItem(
                    "Text Description",
                    Properties: [new InfoProperty("Text", textBlock.Text)]);

            case StopTheTapeIf48KHeader:
                return new InfoItem("Stop Tape (48K)");

            default:
                return new InfoItem(block.Header.Type.ToString());
        }
    }

    // PZX

    [Pure]
    private static List<InfoSection> BuildPzxSections(Pzx.PzxFile file)
    {
        var sections = new List<InfoSection>();
        var blocks = file.Blocks;
        var startIndex = 0;

        if (blocks.Count > 0 && blocks[0] is Pzx.PzxHeaderBlock headerBlock)
        {
            if (headerBlock.Info.Count > 0)
            {
                var infoItems = headerBlock.Info.Select(i =>
                    new InfoItem(i.Type, Properties: [new InfoProperty("Value", i.Text)])
                ).ToList();
                sections.Add(new InfoSection("Info", "file", Items: infoItems));
            }

            startIndex = 1;
        }

        var blockItems = blocks.Skip(startIndex).Select(BuildPzxBlockItem).ToList();
        sections.Add(new InfoSection("Blocks", Items: blockItems));

        return sections;
    }

    [Pure]
    private static InfoItem BuildPzxBlockItem(Pzx.PzxBlock block) => block switch
    {
        Pzx.DataBlock data => new InfoItem(
            "Data",
            Properties: [new InfoProperty("Size", data.Header.SizeInBytes.ToString(NumberFormatInfo.InvariantInfo), "decimal")],
            Details: new Dictionary<string, string>
            {
                ["Initial Pulse Level"] = data.Header.InitialPulseLevel ? "1" : "0",
                ["Extra Bits"] = data.Header.ExtraBits.ToString(NumberFormatInfo.InvariantInfo),
                ["Tail"] = data.Header.Tail.ToString(NumberFormatInfo.InvariantInfo),
                ["Zero Bit Pulses"] = data.Header.NumberOfPulseInZeroBitSequence.ToString(NumberFormatInfo.InvariantInfo),
                ["One Bit Pulses"] = data.Header.NumberOfPulseInOneBitSequence.ToString(NumberFormatInfo.InvariantInfo)
            }),
        Pzx.PulseSequenceBlock pulseSeq => new InfoItem(
            "Pulse Sequence",
            Properties: [new InfoProperty("Pulse Count", pulseSeq.Pulses.Count.ToString(NumberFormatInfo.InvariantInfo), "decimal")]),
        Pzx.PauseBlock pause => new InfoItem(
            "Pause",
            Properties: [new InfoProperty("Duration", $"{pause.Header.Duration} T-States")],
            Details: new Dictionary<string, string>
            {
                ["Initial Pulse Level"] = pause.Header.InitialPulseLevel ? "1" : "0"
            }),
        Pzx.BrowsePointBlock browse => new InfoItem(
            "Browse Point",
            Properties: [new InfoProperty("Text", browse.Text)]),
        Pzx.StopBlock stop => new InfoItem(
            "Stop",
            Properties: [new InfoProperty("48K Only", stop.Header.Only48k.ToString().ToLowerInvariant(), "boolean")]),
        _ => new InfoItem(block.Header.Type.ToString())
    };

    // Snapshots

    [Pure]
    private static List<InfoSection> BuildSnapshotSections(ZXSpectrumSnapshotFile snapshot)
    {
        var sections = new List<InfoSection>();

        switch (snapshot)
        {
            case Z80File z80:
                sections.Add(BuildZ80HardwareSection(z80));
                break;
            case SnaFile sna:
                sections.Add(BuildSnaHardwareSection(sna));
                break;
            case NexFile nex:
                sections.AddRange(BuildNexSections(nex));
                break;
        }

        sections.Add(BuildRegistersSection(snapshot.Registers));
        sections.Add(BuildShadowRegistersSection(snapshot.Registers.Shadow));

        return sections;
    }

    [Pure]
    private static InfoSection BuildZ80HardwareSection(Z80File file)
    {
        var props = new List<InfoProperty>();

        if (file.Header is Z80V2Header v2Header)
        {
            props.Add(new InfoProperty("Hardware Mode", v2Header.HardwareMode.ToString()));
        }

        props.Add(new InfoProperty("Border Colour", file.Header.BorderColour.ToString(), "colour"));
        props.Add(new InfoProperty("Interrupt Mode", file.Header.InterruptMode.ToString(NumberFormatInfo.InvariantInfo), "decimal"));
        props.Add(new InfoProperty("Interrupt Flip-Flop", file.Header.InterruptFlipFlop.ToString().ToLowerInvariant(), "boolean"));
        props.Add(new InfoProperty("IFF2", file.Header.IFF2.ToString().ToLowerInvariant(), "boolean"));
        props.Add(new InfoProperty("Video Synchronisation", file.Header.VideoSynchronisation.ToString()));
        props.Add(new InfoProperty("Joystick", file.Header.Joystick.ToString()));
        props.Add(new InfoProperty("Data Compressed", file.Header.DataIsCompressed.ToString().ToLowerInvariant(), "boolean"));

        return new InfoSection("Hardware", "file", Properties: props);
    }

    [Pure]
    private static InfoSection BuildSnaHardwareSection(SnaFile file) =>
        new("Hardware", "file", Properties:
        [
            new InfoProperty("Border Colour", file.Header.BorderColour.ToString(), "colour"),
            new InfoProperty("Interrupt Mode", file.Header.InterruptMode.ToString(NumberFormatInfo.InvariantInfo), "decimal"),
            new InfoProperty("IFF2", file.Header.IFF2.ToString().ToLowerInvariant(), "boolean")
        ]);

    [Pure]
    private static List<InfoSection> BuildNexSections(NexFile file)
    {
        var sections = new List<InfoSection>
        {
            new("Header", "file", Properties:
            [
                new InfoProperty("Version", file.Header.VersionString),
                new InfoProperty("RAM Required", file.Header.RamRequired.ToString()),
                new InfoProperty("Border Colour", file.Header.BorderColour.ToString(), "colour"),
                new InfoProperty("Entry Bank", file.Header.EntryBank.ToString(NumberFormatInfo.InvariantInfo), "decimal"),
                new InfoProperty("Core Version", $"{file.Header.CoreVersionMajor}.{file.Header.CoreVersionMinor}.{file.Header.CoreVersionSubMinor}"),
                new InfoProperty("Expansion Bus", file.Header.ExpansionBusEnable.ToString().ToLowerInvariant(), "boolean"),
                new InfoProperty("Preserve Next Registers", file.Header.PreserveNextRegisters.ToString().ToLowerInvariant(), "boolean")
            ]),
            new("Screens", "file", Properties:
            [
                new InfoProperty("Layer 2", file.Header.HasLayer2Screen.ToString().ToLowerInvariant(), "boolean"),
                new InfoProperty("ULA", file.Header.HasUlaScreen.ToString().ToLowerInvariant(), "boolean"),
                new InfoProperty("LoRes", file.Header.HasLoResScreen.ToString().ToLowerInvariant(), "boolean"),
                new InfoProperty("HiRes", file.Header.HasHiResScreen.ToString().ToLowerInvariant(), "boolean"),
                new InfoProperty("HiColour", file.Header.HasHiColourScreen.ToString().ToLowerInvariant(), "boolean")
            ])
        };

        if (file.Banks.Count > 0)
        {
            var bankItems = file.Banks.Select(b =>
                new InfoItem($"Bank {b.BankNumber}", Properties: [new InfoProperty("Size", b.Data.Length.ToString(NumberFormatInfo.InvariantInfo), "decimal")])
            ).ToList();
            sections.Add(new InfoSection("Banks", Items: bankItems));
        }

        return sections;
    }

    [Pure]
    private static InfoSection BuildRegistersSection(RegisterSnapshot registers) =>
        new("Registers", Properties:
        [
            new InfoProperty("AF", $"0x{registers.AF:X4}", "hex"),
            new InfoProperty("BC", $"0x{registers.BC:X4}", "hex"),
            new InfoProperty("DE", $"0x{registers.DE:X4}", "hex"),
            new InfoProperty("HL", $"0x{registers.HL:X4}", "hex"),
            new InfoProperty("IX", $"0x{registers.IX:X4}", "hex"),
            new InfoProperty("IY", $"0x{registers.IY:X4}", "hex"),
            new InfoProperty("PC", $"0x{registers.PC:X4}", "hex"),
            new InfoProperty("SP", $"0x{registers.SP:X4}", "hex"),
            new InfoProperty("IR", $"0x{registers.IR:X4}", "hex")
        ]);

    [Pure]
    private static InfoSection BuildShadowRegistersSection(ShadowRegisterSnapshot shadow) =>
        new("Shadow Registers", Properties:
        [
            new InfoProperty("AF'", $"0x{shadow.AF:X4}", "hex"),
            new InfoProperty("BC'", $"0x{shadow.BC:X4}", "hex"),
            new InfoProperty("DE'", $"0x{shadow.DE:X4}", "hex"),
            new InfoProperty("HL'", $"0x{shadow.HL:X4}", "hex")
        ]);

    // Text formatting

    private static void WriteFileInfo(TextWriter output, FileInfoResult fileInfo)
    {
        output.WriteLine($"Format: {fileInfo.Format}");

        foreach (var section in fileInfo.Sections)
        {
            WriteSection(output, section, "");
        }
    }

    private static void WriteSection(TextWriter output, InfoSection section, string indent)
    {
        if (section.Properties != null)
        {
            output.WriteLine($"{indent}{section.Title}:");
            foreach (var prop in section.Properties)
            {
                output.WriteLine($"{indent}  {prop.Name}: {prop.Value}");
            }
        }

        if (section.Items != null)
        {
            output.WriteLine($"{indent}{section.Title}: {section.Items.Count}");
            foreach (var (item, index) in section.Items.Select((item, i) => (item, i + 1)))
            {
                output.Write($"{indent}  {index}: {item.Title}");

                if (item.Properties != null)
                {
                    var propSummary = string.Join(", ", item.Properties.Select(p => $"{p.Name}: {p.Value}"));
                    output.Write($" ({propSummary})");
                }

                output.WriteLine();

                if (item.Details != null)
                {
                    foreach (var (key, value) in item.Details)
                    {
                        output.WriteLine($"{indent}     {key}: {value}");
                    }
                }

                if (item.Sections != null)
                {
                    foreach (var nestedSection in item.Sections)
                    {
                        WriteSection(output, nestedSection, indent + "     ");
                    }
                }
            }
        }
    }
}