namespace MrKWatkins.OakIO.Commands.FileInfo;

/// <summary>
/// A file format that this file can be converted to.
/// </summary>
public sealed record ConvertibleFormat(
    string Name,
    string Extension);