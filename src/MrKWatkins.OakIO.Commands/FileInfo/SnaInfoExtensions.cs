using System.Globalization;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

namespace MrKWatkins.OakIO.Commands.FileInfo;

internal static class SnaInfoExtensions
{
    [Pure]
    internal static InfoSection ToHardwareInfoSection(this SnaFile file) =>
        new(Info.Sections.Hardware)
        {
            Properties =
            [
                new InfoProperty(Info.Properties.BorderColour, file.Header.BorderColour.ToString(), Info.Formats.Colour),
                new InfoProperty(Info.Properties.InterruptMode, file.Header.InterruptMode.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal),
                new InfoProperty(Info.Properties.IFF2, file.Header.IFF2.ToString().ToLowerInvariant(), Info.Formats.Boolean)
            ]
        };
}