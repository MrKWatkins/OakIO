using System.Runtime.CompilerServices;
using MrKWatkins.OakIO.Resources.Gpl;
using MrKWatkins.OakIO.Resources.Jasc;
using MrKWatkins.OakIO.Resources.PaintNet;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources;

internal static class ResourceConversions
{
    // Register cross-assembly conversions when Next is loaded, including before an NxpFile has been created.
    // This keeps core OakIO independent of Next and retains the existing internal registration mechanism.
    [ModuleInitializer]
    [SuppressMessage("Usage", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries",
        Justification = "Register built-in converters supplied by this optional format assembly without a dependency from core OakIO.")]
    internal static void Initialize() =>
        IOFileConversion.RegisterConverters(
            new PaletteToNxpConverter(GplFormat.Instance),
            new PaletteToNxpConverter(JascFormat.Instance),
            new PaletteToNxpConverter(PaintNetFormat.Instance));
}