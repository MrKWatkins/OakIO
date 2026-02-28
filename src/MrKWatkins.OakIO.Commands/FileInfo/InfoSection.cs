namespace MrKWatkins.OakIO.Commands.FileInfo;

/// <summary>
/// A section of related information, containing properties and/or repeated items.
/// </summary>
public sealed record InfoSection(string Title, string Category = "content")
{
    public IReadOnlyList<InfoProperty> Properties { get; init; } = [];

    public IReadOnlyList<InfoItem> Items { get; init; } = [];
}