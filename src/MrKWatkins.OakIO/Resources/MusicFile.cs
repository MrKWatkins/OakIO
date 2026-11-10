namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for music files whose concrete components represent their on-disk format.
/// </summary>
public abstract class MusicFile : ResourceFile
{
    /// <summary>
    /// Initializes a music file.
    /// </summary>
    /// <param name="format">The file format.</param>
    protected MusicFile(MusicFormat format)
        : base(format ?? throw new ArgumentNullException(nameof(format)))
    {
    }

    /// <summary>
    /// Gets the music file format.
    /// </summary>
    public new MusicFormat Format => (MusicFormat)base.Format;
}