namespace MrKWatkins.OakIO.Commands.FileInfo;

/// <summary>
/// Structured information about a file.
/// </summary>
public sealed record FileInfoResult(
    string Format,
    string FileExtension,
    string Type,
    IReadOnlyList<ConvertibleFormat> ConvertibleTo,
    IReadOnlyList<InfoSection> Sections)
{
    /// <summary>
    /// Gets palette colours in index order for palettes and indexed images.
    /// </summary>
    public IReadOnlyList<Resources.Colour>? Palette { get; init; }

    /// <summary>
    /// Gets the decoded image preview, when this file is an image.
    /// </summary>
    public ImagePreview? Image { get; init; }
}