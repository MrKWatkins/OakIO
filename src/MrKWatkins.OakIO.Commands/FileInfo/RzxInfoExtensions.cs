using System.Globalization;
using MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

namespace MrKWatkins.OakIO.Commands.FileInfo;

internal static class RzxInfoExtensions
{
    [Pure]
    internal static IReadOnlyList<InfoSection> ToInfoSections(this RzxFile file)
    {
        var headerSection = new InfoSection(Info.Sections.Header, Info.Icons.File)
        {
            Properties = [new InfoProperty(Info.Properties.Version, $"{file.Header.MajorVersion}.{file.Header.MinorVersion}")]
        };

        var blockItems = file.Blocks.Select(ToInfoItem).ToList();

        return [headerSection, new InfoSection(Info.Sections.Blocks) { Items = blockItems }];
    }

    [Pure]
    private static InfoItem ToInfoItem(this RzxBlock block) => block switch
    {
        RzxCreatorBlock creator => new InfoItem($"{Info.Items.Creator}: {creator.Creator}")
        {
            Properties = [new InfoProperty(Info.Properties.Version, $"{creator.MajorVersion}.{creator.MinorVersion}")]
        },
        RzxSnapshotBlock snapshot => new InfoItem(Info.Items.Snapshot)
        {
            Properties =
            [
                new InfoProperty(Info.Properties.Extension, snapshot.Extension),
                new InfoProperty(Info.Properties.Size, snapshot.UncompressedLength.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal)
            ]
        },
        RzxInputRecordingBlock recording => new InfoItem(Info.Items.InputRecording)
        {
            Properties =
            [
                new InfoProperty(Info.Properties.Frames, recording.Frames.Count.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal),
                new InfoProperty(Info.Properties.StartTStates, recording.StartTStates.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal)
            ]
        },
        _ => throw new NotSupportedException($"The RZX block type {block.GetType().Name} is not supported.")
    };
}