namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for music files whose concrete components represent their on-disk format.
/// </summary>
/// <param name="format">The file format.</param>
public abstract class MusicFile(MusicFormat format) : ResourceFile(format ?? throw new ArgumentNullException(nameof(format)))
{

    /// <summary>
    /// Gets the music file format.
    /// </summary>
    public new MusicFormat Format => (MusicFormat)base.Format;
}