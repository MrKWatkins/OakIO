namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for image files whose concrete components represent their on-disk format.
/// </summary>
/// <param name="format">The file format.</param>
public abstract class ImageFile(ImageFormat format) : ResourceFile(format ?? throw new ArgumentNullException(nameof(format)))
{

    /// <summary>
    /// Gets the image file format.
    /// </summary>
    public new ImageFormat Format => (ImageFormat)base.Format;

    /// <summary>
    /// Gets a shared convenience view derived from the file's concrete components.
    /// </summary>
    public abstract ImageData Image { get; }
}