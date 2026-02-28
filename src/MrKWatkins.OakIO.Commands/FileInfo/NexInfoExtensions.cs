using System.Globalization;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

namespace MrKWatkins.OakIO.Commands.FileInfo;

internal static class NexInfoExtensions
{
    [Pure]
    internal static IReadOnlyList<InfoSection> ToInfoSections(this NexFile file)
    {
        var sections = new List<InfoSection>
        {
            new(Info.Sections.Header, Info.Icons.File)
            {
                Properties =
                [
                    new InfoProperty(Info.Properties.Version, file.Header.VersionString),
                    new InfoProperty(Info.Properties.RamRequired, file.Header.RamRequired.ToString()),
                    new InfoProperty(Info.Properties.BorderColour, file.Header.BorderColour.ToString(), Info.Formats.Colour),
                    new InfoProperty(Info.Properties.EntryBank, file.Header.EntryBank.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal),
                    new InfoProperty(Info.Properties.CoreVersion, $"{file.Header.CoreVersionMajor}.{file.Header.CoreVersionMinor}.{file.Header.CoreVersionSubMinor}"),
                    new InfoProperty(Info.Properties.ExpansionBus, file.Header.ExpansionBusEnable.ToString().ToLowerInvariant(), Info.Formats.Boolean),
                    new InfoProperty(Info.Properties.PreserveNextRegisters, file.Header.PreserveNextRegisters.ToString().ToLowerInvariant(), Info.Formats.Boolean)
                ]
            },
            new(Info.Sections.Screens, Info.Icons.File)
            {
                Properties =
                [
                    new InfoProperty(Info.Properties.Layer2, file.Header.HasLayer2Screen.ToString().ToLowerInvariant(), Info.Formats.Boolean),
                    new InfoProperty(Info.Properties.ULA, file.Header.HasUlaScreen.ToString().ToLowerInvariant(), Info.Formats.Boolean),
                    new InfoProperty(Info.Properties.LoRes, file.Header.HasLoResScreen.ToString().ToLowerInvariant(), Info.Formats.Boolean),
                    new InfoProperty(Info.Properties.HiRes, file.Header.HasHiResScreen.ToString().ToLowerInvariant(), Info.Formats.Boolean),
                    new InfoProperty(Info.Properties.HiColour, file.Header.HasHiColourScreen.ToString().ToLowerInvariant(), Info.Formats.Boolean)
                ]
            }
        };

        if (file.Banks.Count > 0)
        {
            var bankItems = file.Banks.Select(b =>
                new InfoItem($"Bank {b.BankNumber}") { Properties = [new InfoProperty(Info.Properties.Size, b.Data.Length.ToString(NumberFormatInfo.InvariantInfo), Info.Formats.Decimal)] }
            ).ToList();
            sections.Add(new InfoSection(Info.Sections.Banks) { Items = bankItems });
        }

        return sections;
    }
}