using System.Globalization;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.Commands.FileInfo;

internal static class TapInfoExtensions
{
    [Pure]
    internal static IReadOnlyList<InfoSection> ToInfoSections(this TapFile file)
    {
        var items = file.Blocks.Select(ToInfoItem).ToList();
        return [new InfoSection(Info.Sections.Blocks) { Items = items }];
    }

    [Pure]
    private static InfoItem ToInfoItem(this TapBlock block) => block switch
    {
        HeaderBlock header => new InfoItem(header.ToString())
        {
            Properties =
            [
                new InfoProperty(Info.Properties.Type, "Header"),
                new InfoProperty(Info.Properties.HeaderType, header.HeaderType.ToString()),
                new InfoProperty(Info.Properties.Filename, header.Filename),
                new InfoProperty(Info.Properties.DataLength, header.DataBlockLength.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal),
                ..header.HeaderType is TapHeaderType.Program or TapHeaderType.Code
                    ? [new InfoProperty(Info.Properties.Location, header.Location.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal)]
                    : Array.Empty<InfoProperty>()
            ]
        },
        DataBlock data => new InfoItem(data.ToString())
        {
            Properties =
            [
                new InfoProperty(Info.Properties.Type, "Data"),
                new InfoProperty(Info.Properties.Length, data.Header.BlockLength.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal)
            ]
        },
        _ => new InfoItem(block.ToString() ?? block.GetType().Name)
    };
}