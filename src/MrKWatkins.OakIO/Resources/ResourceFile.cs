namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for resource files.
/// </summary>
/// <param name="format">The file format.</param>
public abstract class ResourceFile(ResourceFormat format) : IOFile(format ?? throw new ArgumentNullException(nameof(format)))
{

    /// <summary>
    /// Gets the resource file format.
    /// </summary>
    public new ResourceFormat Format => (ResourceFormat)base.Format;
}