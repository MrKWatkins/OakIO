namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for image files whose concrete components represent their on-disk format.
/// </summary>
public abstract class ImageFile : ResourceFile
{
    /// <summary>
    /// Initializes an image file.
    /// </summary>
    /// <param name="format">The file format.</param>
    protected ImageFile(ImageFormat format)
        : base(format ?? throw new ArgumentNullException(nameof(format)))
    {
    }

    /// <summary>
    /// Gets the image file format.
    /// </summary>
    public new ImageFormat Format => (ImageFormat)base.Format;

    /// <summary>
    /// Gets a shared convenience view derived from the file's concrete components.
    /// </summary>
    public abstract ImageData Image { get; }
}
