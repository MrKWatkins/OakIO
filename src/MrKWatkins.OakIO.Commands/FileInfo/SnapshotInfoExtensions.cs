using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.Commands.FileInfo;

internal static class SnapshotInfoExtensions
{
    [Pure]
    internal static IReadOnlyList<InfoSection> ToInfoSections(this ZXSpectrumSnapshotFile snapshot)
    {
        var sections = new List<InfoSection>();

        switch (snapshot)
        {
            case Z80File z80:
                sections.Add(z80.ToHardwareInfoSection());
                break;
            case SnaFile sna:
                sections.Add(sna.ToHardwareInfoSection());
                break;
            case NexFile nex:
                sections.AddRange(nex.ToInfoSections());
                break;
        }

        sections.Add(snapshot.Registers.ToInfoSection());
        if (snapshot is not NexFile)
        {
            sections.Add(snapshot.Registers.Shadow.ToInfoSection());
        }

        return sections;
    }

    [Pure]
    internal static InfoSection ToInfoSection(this RegisterSnapshot registers) =>
        new(Info.Sections.Registers)
        {
            Properties =
            [
                new InfoProperty(Info.Properties.AF, $"0x{registers.AF:X4}", Info.Formats.Hex),
                new InfoProperty(Info.Properties.BC, $"0x{registers.BC:X4}", Info.Formats.Hex),
                new InfoProperty(Info.Properties.DE, $"0x{registers.DE:X4}", Info.Formats.Hex),
                new InfoProperty(Info.Properties.HL, $"0x{registers.HL:X4}", Info.Formats.Hex),
                new InfoProperty(Info.Properties.IX, $"0x{registers.IX:X4}", Info.Formats.Hex),
                new InfoProperty(Info.Properties.IY, $"0x{registers.IY:X4}", Info.Formats.Hex),
                new InfoProperty(Info.Properties.PC, $"0x{registers.PC:X4}", Info.Formats.Hex),
                new InfoProperty(Info.Properties.SP, $"0x{registers.SP:X4}", Info.Formats.Hex),
                new InfoProperty(Info.Properties.IR, $"0x{registers.IR:X4}", Info.Formats.Hex)
            ]
        };

    [Pure]
    internal static InfoSection ToInfoSection(this ShadowRegisterSnapshot shadow) =>
        new(Info.Sections.PrimeRegisters)
        {
            Properties =
            [
                new InfoProperty(Info.Properties.ShadowAF, $"0x{shadow.AF:X4}", Info.Formats.Hex),
                new InfoProperty(Info.Properties.ShadowBC, $"0x{shadow.BC:X4}", Info.Formats.Hex),
                new InfoProperty(Info.Properties.ShadowDE, $"0x{shadow.DE:X4}", Info.Formats.Hex),
                new InfoProperty(Info.Properties.ShadowHL, $"0x{shadow.HL:X4}", Info.Formats.Hex)
            ]
        };
}