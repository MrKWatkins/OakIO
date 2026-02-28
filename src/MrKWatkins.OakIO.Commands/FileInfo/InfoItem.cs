using System.Collections.Frozen;

namespace MrKWatkins.OakIO.Commands.FileInfo;

/// <summary>
/// An item within a section, such as a block in a tape file or a bank in a snapshot.
/// </summary>
public sealed record InfoItem(string Title)
{
    public IReadOnlyList<InfoProperty> Properties { get; init; } = [];

    public IReadOnlyDictionary<string, string> Details { get; init; } = FrozenDictionary<string, string>.Empty;

    public IReadOnlyList<InfoSection> Sections { get; init; } = [];
}