using System.Globalization;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.Commands.FileInfo;

internal static class Z80InfoExtensions
{
    [Pure]
    internal static InfoSection ToHardwareInfoSection(this Z80File file)
    {
        var props = new List<InfoProperty>();

        if (file.Header is Z80V2Header v2Header)
        {
            props.Add(new InfoProperty(Info.Properties.HardwareMode, v2Header.HardwareMode.ToString()));
        }

        props.Add(new InfoProperty(Info.Properties.BorderColour, file.Header.BorderColour.ToString(), Info.Formats.Colour));
        props.Add(new InfoProperty(Info.Properties.InterruptMode, file.Header.InterruptMode.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal));
        props.Add(new InfoProperty(Info.Properties.IFF1, file.Header.IFF1.ToString().ToLowerInvariant(), Info.Formats.Boolean));
        props.Add(new InfoProperty(Info.Properties.IFF2, file.Header.IFF2.ToString().ToLowerInvariant(), Info.Formats.Boolean));
        props.Add(new InfoProperty(Info.Properties.VideoSynchronisation, file.Header.VideoSynchronisation.ToString()));
        props.Add(new InfoProperty(Info.Properties.Joystick, file.Header.Joystick.ToString()));
        props.Add(new InfoProperty(Info.Properties.DataCompressed, file.Header.DataIsCompressed.ToString().ToLowerInvariant(), Info.Formats.Boolean));

        return new InfoSection(Info.Sections.Hardware) { Properties = props };
    }
}