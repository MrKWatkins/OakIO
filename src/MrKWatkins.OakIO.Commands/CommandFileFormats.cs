using MrKWatkins.OakIO.ZXSpectrumNext;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources;

namespace MrKWatkins.OakIO.Commands;

internal static class CommandFileFormats
{
    public static readonly IReadOnlyList<IOFileFormat> AllFormats =
        [.. ZXSpectrumNextFileFormats.AllFormats, .. ZXSpectrumNextResourceFileFormats.AllFormats];
}