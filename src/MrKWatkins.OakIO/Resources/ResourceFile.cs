namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for resource files.
/// </summary>
public abstract class ResourceFile : IOFile
{
    /// <summary>
    /// Initializes a resource file.
    /// </summary>
    /// <param name="format">The file format.</param>
    protected ResourceFile(ResourceFormat format)
        : base(format ?? throw new ArgumentNullException(nameof(format)))
    {
    }

    /// <summary>
    /// Gets the resource file format.
    /// </summary>
    public new ResourceFormat Format => (ResourceFormat)base.Format;
}