namespace MrKWatkins.OakIO.Commands.FileInfo;

/// <summary>
/// Structured information about a file.
/// </summary>
public sealed record FileInfoResult(
    string Format,
    string FileExtension,
    string Type,
    IReadOnlyList<ConvertibleFormat> ConvertibleTo,
    IReadOnlyList<InfoSection> Sections);