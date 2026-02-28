using System.Globalization;
using MrKWatkins.OakIO.ZXSpectrum.Tape;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

namespace MrKWatkins.OakIO.Commands.FileInfo;

internal static class TzxInfoExtensions
{
    [Pure]
    internal static IReadOnlyList<InfoSection> ToInfoSections(this TzxFile file)
    {
        var sections = new List<InfoSection>();

        var archiveInfoBlock = file.Blocks.OfType<ArchiveInfoBlock>().FirstOrDefault();
        if (archiveInfoBlock != null)
        {
            var properties = archiveInfoBlock.Entries.Select(e =>
                new InfoProperty(e.Type.ToDescription(), e.Text)
            ).ToList();
            sections.Add(new InfoSection(Info.Sections.ArchiveInfo, Info.Icons.File) { Properties = properties });
        }

        var blocks = file.Blocks.Where(b => b is not ArchiveInfoBlock).ToList();
        var index = 0;
        var items = BuildItems(blocks, ref index);
        sections.Add(new InfoSection(Info.Sections.Blocks) { Items = items });

        return sections;
    }

    private static List<InfoItem> BuildItems(List<TzxBlock> blocks, ref int index)
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
                var body = BuildItems(blocks, ref index);
                items.Add(new InfoItem(Info.Items.Loop)
                {
                    Properties = [new InfoProperty(Info.Properties.Repetitions, loopStart.Header.NumberOfRepetitions.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal)],
                    Sections = [new InfoSection(Info.Sections.Blocks) { Items = body }]
                });
            }
            else if (block is GroupStartBlock groupStart)
            {
                index++;
                var body = BuildItems(blocks, ref index);
                items.Add(new InfoItem($"Group: {groupStart.Text}")
                {
                    Sections = [new InfoSection(Info.Sections.Blocks) { Items = body }]
                });
            }
            else
            {
                items.Add(block.ToInfoItem());
            }

            index++;
        }

        return items;
    }

    [Pure]
    private static InfoItem ToInfoItem(this TzxBlock block)
    {
        switch (block.Header)
        {
            case StandardSpeedDataHeader h:
                var standardSpeedProperties = new List<InfoProperty>
                {
                    new(Info.Properties.Length, h.BlockLength.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal),
                    new(Info.Properties.PauseAfter, $"{h.PauseAfterBlockMs} ms")
                };
                if (block is StandardSpeedDataBlock ssdb && ssdb.TryGetStandardFileHeader(out var fileHeader))
                {
                    AddStandardFileHeaderProperties(standardSpeedProperties, fileHeader);
                }

                return new InfoItem(Info.Items.StandardSpeedData)
                {
                    Properties = standardSpeedProperties
                };

            case TurboSpeedDataHeader h:
                return new InfoItem(Info.Items.TurboSpeedData)
                {
                    Properties =
                    [
                        new InfoProperty(Info.Properties.Length, h.BlockLength.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal),
                        new InfoProperty(Info.Properties.PauseAfter, $"{h.PauseAfterBlockMs} ms")
                    ],
                    Details = new Dictionary<string, string>
                    {
                        [Info.Properties.PilotPulse] = $"{h.TStatesInPilotPulse} T-States",
                        [Info.Properties.PilotTonePulses] = h.PulsesInPilotTone.ToString(NumberFormatInfo.InvariantInfo),
                        [Info.Properties.SyncFirstPulse] = $"{h.TStatesInSyncFirstPulse} T-States",
                        [Info.Properties.SyncSecondPulse] = $"{h.TStatesInSyncSecondPulse} T-States",
                        [Info.Properties.ZeroBitPulse] = $"{h.TStatesInZeroBitPulse} T-States",
                        [Info.Properties.OneBitPulse] = $"{h.TStatesInOneBitPulse} T-States",
                        [Info.Properties.UsedBitsInLastByte] = h.UsedBitsInLastByte.ToString(NumberFormatInfo.InvariantInfo)
                    }
                };

            case PureDataHeader h:
                return new InfoItem(Info.Items.PureData)
                {
                    Properties =
                    [
                        new InfoProperty(Info.Properties.Length, h.BlockLength.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal),
                        new InfoProperty(Info.Properties.PauseAfter, $"{h.PauseAfterBlockMs} ms")
                    ],
                    Details = new Dictionary<string, string>
                    {
                        [Info.Properties.ZeroBitPulse] = $"{h.TStatesInZeroBitPulse} T-States",
                        [Info.Properties.OneBitPulse] = $"{h.TStatesInOneBitPulse} T-States",
                        [Info.Properties.UsedBitsInLastByte] = h.UsedBitsInLastByte.ToString(NumberFormatInfo.InvariantInfo)
                    }
                };

            case PureToneHeader h:
                return new InfoItem(Info.Items.PureTone)
                {
                    Properties =
                    [
                        new InfoProperty(Info.Properties.PulseCount, h.NumberOfPulses.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal),
                        new InfoProperty(Info.Properties.PulseLength, $"{h.LengthOfPulse} T-States")
                    ]
                };

            case PulseSequenceHeader h:
                return new InfoItem(Info.Items.PulseSequence)
                {
                    Properties = [new InfoProperty(Info.Properties.PulseCount, h.NumberOfPulses.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal)]
                };

            case PauseHeader h:
                return new InfoItem(Info.Items.Pause)
                {
                    Properties = [new InfoProperty(Info.Properties.Duration, $"{h.PauseMs} ms")]
                };

            case TextDescriptionHeader when block is TextDescriptionBlock textBlock:
                return new InfoItem(Info.Items.Text)
                {
                    Properties = [new InfoProperty(Info.Properties.Text, textBlock.Text)]
                };

            case StopTheTapeIf48KHeader:
                return new InfoItem(Info.Items.StopTape48K);

            default:
                throw new NotSupportedException($"The TZX block header type {block.Header.Type} is not supported.");
        }
    }

    private static void AddStandardFileHeaderProperties(List<InfoProperty> properties, StandardFileHeader header)
    {
        properties.Add(new InfoProperty(Info.Properties.HeaderType, header.Type.ToString()));
        properties.Add(new InfoProperty(Info.Properties.Filename, header.Filename));
        properties.Add(new InfoProperty(Info.Properties.DataLength, header.DataBlockLength.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal));
    }
}