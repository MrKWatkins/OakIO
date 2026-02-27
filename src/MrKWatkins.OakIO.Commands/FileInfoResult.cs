using System.Text.Json.Serialization;

namespace MrKWatkins.OakIO.Commands;

/// <summary>
/// Structured information about a file.
/// </summary>
public sealed record FileInfoResult(
    string Format,
    string FileExtension,
    string Type,
    IReadOnlyList<ConvertibleFormat> ConvertibleTo,
    IReadOnlyList<InfoSection> Sections);

/// <summary>
/// A file format that this file can be converted to.
/// </summary>
public sealed record ConvertibleFormat(
    string Name,
    string Extension);

/// <summary>
/// A section of related information, containing properties and/or repeated items.
/// </summary>
public sealed record InfoSection(
    string Title,
    string Category = "content",
    IReadOnlyList<InfoProperty>? Properties = null,
    IReadOnlyList<InfoItem>? Items = null);

/// <summary>
/// A property with a name, formatted value, and optional format hint for rendering.
/// </summary>
public sealed record InfoProperty(
    string Name,
    string Value,
    string? Format = null);

/// <summary>
/// An item within a section, such as a block in a tape file or a bank in a snapshot.
/// </summary>
public sealed record InfoItem(
    string Title,
    IReadOnlyList<InfoProperty>? Properties = null,
    IReadOnlyDictionary<string, string>? Details = null,
    IReadOnlyList<InfoSection>? Sections = null);

[JsonSerializable(typeof(FileInfoResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class FileInfoJsonContext : JsonSerializerContext;