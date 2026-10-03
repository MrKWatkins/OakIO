using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Supported general resource formats, separate from snapshot, tape and recording formats.
/// </summary>
public static class ResourceFileFormats
{
    /// <summary>
    /// Gets all supported general resource formats.
    /// </summary>
    public static readonly IReadOnlyList<ResourceFormat> AllFormats = [BmpFormat.Instance];
}