using MrKWatkins.OakIO.Resources.Bmp;
using MrKWatkins.OakIO.Resources.Gpl;
using MrKWatkins.OakIO.Resources.Jasc;
using MrKWatkins.OakIO.Resources.PaintNet;

namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Supported general resource formats, separate from snapshot, tape and recording formats.
/// </summary>
public static class ResourceFileFormats
{
    /// <summary>
    /// Gets all supported general resource formats.
    /// </summary>
    public static readonly IReadOnlyList<ResourceFormat> AllFormats = [BmpFormat.Instance, GplFormat.Instance, JascFormat.Instance, PaintNetFormat.Instance];
}