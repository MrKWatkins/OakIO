namespace MrKWatkins.OakIO.Commands.FileInfo;

/// <summary>
/// A property with a name, formatted value, and optional format hint for rendering.
/// </summary>
public sealed record InfoProperty(
    string Name,
    string Value,
    string? Format = null);