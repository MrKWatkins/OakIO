using System.Text.Json.Serialization;

namespace MrKWatkins.OakIO.Commands.FileInfo;

[JsonSerializable(typeof(FileInfoResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class FileInfoJsonContext : JsonSerializerContext;