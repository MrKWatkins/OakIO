using System.Runtime.CompilerServices;
using MrKWatkins.OakIO.Resources.Bmp;
using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

namespace MrKWatkins.OakIO.ZXSpectrum.Resources;

internal static class ResourceConversions
{
    // Register incoming optional-assembly conversions before any SCR file is created, without a core dependency on Spectrum.
    [ModuleInitializer]
    [SuppressMessage("Usage", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries",
        Justification = "Register built-in converters supplied by this optional format assembly without a dependency from core OakIO.")]
    internal static void Initialize() =>
        IOFileConversion.RegisterConverters(new ImageToScrConverter(BmpFormat.Instance), new ImageToScrConverter(ScrFormat.Instance));
}