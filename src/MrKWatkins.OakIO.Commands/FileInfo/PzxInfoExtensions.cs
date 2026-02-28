using System.Globalization;
using Pzx = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

namespace MrKWatkins.OakIO.Commands.FileInfo;

internal static class PzxInfoExtensions
{
    [Pure]
    internal static IReadOnlyList<InfoSection> ToInfoSections(this Pzx.PzxFile file)
    {
        var sections = new List<InfoSection>();
        var blocks = file.Blocks;
        var startIndex = 0;

        if (blocks.Count > 0 && blocks[0] is Pzx.PzxHeaderBlock headerBlock)
        {
            if (headerBlock.Info.Count > 0)
            {
                var infoItems = headerBlock.Info.Select(i =>
                    new InfoItem(i.Type) { Properties = [new InfoProperty(Info.Properties.Value, i.Text)] }
                ).ToList();
                sections.Add(new InfoSection(Info.Sections.Info, Info.Icons.File) { Items = infoItems });
            }

            startIndex = 1;
        }

        var blockItems = blocks.Skip(startIndex).Select(ToInfoItem).ToList();
        sections.Add(new InfoSection(Info.Sections.Blocks) { Items = blockItems });

        return sections;
    }

    [Pure]
    private static InfoItem ToInfoItem(this Pzx.PzxBlock block) => block switch
    {
        Pzx.DataBlock data => CreateDataInfoItem(data),
        Pzx.PulseSequenceBlock pulseSeq => new InfoItem(Info.Items.PulseSequence)
        {
            Properties = [new InfoProperty(Info.Properties.PulseCount, pulseSeq.Pulses.Count.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal)]
        },
        Pzx.PauseBlock pause => new InfoItem(Info.Items.Pause)
        {
            Properties = [new InfoProperty(Info.Properties.Duration, $"{pause.Header.Duration} T-States")],
            Details = new Dictionary<string, string>
            {
                [Info.Properties.InitialPulseLevel] = pause.Header.InitialPulseLevel ? "1" : "0"
            }
        },
        Pzx.BrowsePointBlock browse => new InfoItem(Info.Items.BrowsePoint)
        {
            Properties = [new InfoProperty(Info.Properties.Text, browse.Text)]
        },
        Pzx.StopBlock stop => new InfoItem(Info.Items.Stop)
        {
            Properties = [new InfoProperty(Info.Properties.Only48K, stop.Header.Only48k.ToString().ToLowerInvariant(), Info.Formats.Boolean)]
        },
        _ => new InfoItem(block.Header.Type.ToString())
    };

    [Pure]
    private static InfoItem CreateDataInfoItem(Pzx.DataBlock data)
    {
        var properties = new List<InfoProperty>
        {
            new(Info.Properties.Size, data.Header.SizeInBytes.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal)
        };

        if (data.TryGetStandardFileHeader(out var fileHeader))
        {
            properties.Add(new InfoProperty(Info.Properties.HeaderType, fileHeader.Type.ToString()));
            properties.Add(new InfoProperty(Info.Properties.Filename, fileHeader.Filename));
            properties.Add(new InfoProperty(Info.Properties.DataLength, fileHeader.DataBlockLength.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal));
        }

        return new InfoItem(Info.Items.Data)
        {
            Properties = properties,
            Details = new Dictionary<string, string>
            {
                [Info.Properties.InitialPulseLevel] = data.Header.InitialPulseLevel ? "1" : "0",
                [Info.Properties.ExtraBits] = data.Header.ExtraBits.ToString(NumberFormatInfo.InvariantInfo),
                [Info.Properties.Tail] = data.Header.Tail.ToString(NumberFormatInfo.InvariantInfo),
                [Info.Properties.ZeroBitPulses] = data.Header.NumberOfPulseInZeroBitSequence.ToString(NumberFormatInfo.InvariantInfo),
                [Info.Properties.OneBitPulses] = data.Header.NumberOfPulseInOneBitSequence.ToString(NumberFormatInfo.InvariantInfo)
            }
        };
    }
}