namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for image files.
/// </summary>
public abstract class ImageFile : ResourceFile
{
    /// <summary>
    /// Initializes a image file.
    /// </summary>
    /// <param name="format">The file format.</param>
    /// <param name="image">The shared image data.</param>
    protected ImageFile(ImageFormat format, ImageData image)
        : base(format ?? throw new ArgumentNullException(nameof(format)))
    {
        ArgumentNullException.ThrowIfNull(image);
        Image = image;
    }

    /// <summary>
    /// Gets the image file format.
    /// </summary>
    public new ImageFormat Format => (ImageFormat)base.Format;

    /// <summary>
    /// Gets the shared image data.
    /// </summary>
    public ImageData Image { get; }
}