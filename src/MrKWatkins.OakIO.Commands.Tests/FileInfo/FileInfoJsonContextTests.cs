using System.Text.Json;
using MrKWatkins.OakIO.Commands.FileInfo;

namespace MrKWatkins.OakIO.Commands.Tests.FileInfo;

public sealed class FileInfoJsonContextTests : CommandsTestFixture
{
    [Test]
    public void RoundTrip_TapFile()
    {
        using var inputFile = CreateTapFile();
        var original = InfoCommand.GetFileInfo(inputFile.Path, inputFile.Bytes);

        var json = JsonSerializer.Serialize(original, FileInfoJsonContext.Default.FileInfoResult);
        var deserialized = JsonSerializer.Deserialize(json, FileInfoJsonContext.Default.FileInfoResult);

        deserialized.Should().NotBeNull();
        deserialized!.Format.Should().Equal(original.Format);
        deserialized.FileExtension.Should().Equal(original.FileExtension);
        deserialized.Type.Should().Equal(original.Type);
        deserialized.ConvertibleTo.Count.Should().Equal(original.ConvertibleTo.Count);
        deserialized.Sections.Count.Should().Equal(original.Sections.Count);
    }

    [Test]
    public void RoundTrip_TzxFile()
    {
        using var inputFile = CreateTzxFile();
        var original = InfoCommand.GetFileInfo(inputFile.Path, inputFile.Bytes);

        var json = JsonSerializer.Serialize(original, FileInfoJsonContext.Default.FileInfoResult);
        var deserialized = JsonSerializer.Deserialize(json, FileInfoJsonContext.Default.FileInfoResult);

        deserialized.Should().NotBeNull();
        deserialized!.Format.Should().Equal(original.Format);
        deserialized.Sections.Count.Should().Equal(original.Sections.Count);

        var blocksSection = deserialized.Sections[0];
        blocksSection.Title.Should().Equal("Blocks");
        blocksSection.Items.Count.Should().Equal(original.Sections[0].Items.Count);
    }

    [Test]
    public void RoundTrip_Z80File()
    {
        using var inputFile = CreateZ80File();
        var original = InfoCommand.GetFileInfo(inputFile.Path, inputFile.Bytes);

        var json = JsonSerializer.Serialize(original, FileInfoJsonContext.Default.FileInfoResult);
        var deserialized = JsonSerializer.Deserialize(json, FileInfoJsonContext.Default.FileInfoResult);

        deserialized.Should().NotBeNull();
        deserialized!.Format.Should().Equal(original.Format);
        deserialized.Type.Should().Equal("snapshot");
        deserialized.Sections.Count.Should().Equal(original.Sections.Count);

        var registersSection = deserialized.Sections.Single(s => s.Title == "Registers");
        registersSection.Properties.Count.Should().Equal(original.Sections.Single(s => s.Title == "Registers").Properties.Count);
    }

    [Test]
    public void RoundTrip_PreservesConvertibleFormats()
    {
        using var inputFile = CreateTapFile();
        var original = InfoCommand.GetFileInfo(inputFile.Path, inputFile.Bytes);

        var json = JsonSerializer.Serialize(original, FileInfoJsonContext.Default.FileInfoResult);
        var deserialized = JsonSerializer.Deserialize(json, FileInfoJsonContext.Default.FileInfoResult);

        deserialized.Should().NotBeNull();
        for (var i = 0; i < original.ConvertibleTo.Count; i++)
        {
            deserialized!.ConvertibleTo[i].Name.Should().Equal(original.ConvertibleTo[i].Name);
            deserialized.ConvertibleTo[i].Extension.Should().Equal(original.ConvertibleTo[i].Extension);
        }
    }

    [Test]
    public void RoundTrip_PreservesProperties()
    {
        using var inputFile = CreateZ80File();
        var original = InfoCommand.GetFileInfo(inputFile.Path, inputFile.Bytes);
        var originalRegisters = original.Sections.Single(s => s.Title == "Registers");

        var json = JsonSerializer.Serialize(original, FileInfoJsonContext.Default.FileInfoResult);
        var deserialized = JsonSerializer.Deserialize(json, FileInfoJsonContext.Default.FileInfoResult);

        deserialized.Should().NotBeNull();
        var deserializedRegisters = deserialized!.Sections.Single(s => s.Title == "Registers");
        for (var i = 0; i < originalRegisters.Properties.Count; i++)
        {
            deserializedRegisters.Properties[i].Name.Should().Equal(originalRegisters.Properties[i].Name);
            deserializedRegisters.Properties[i].Value.Should().Equal(originalRegisters.Properties[i].Value);
            deserializedRegisters.Properties[i].Format.Should().Equal(originalRegisters.Properties[i].Format);
        }
    }

    [Test]
    public void RoundTrip_PreservesItemProperties()
    {
        using var inputFile = CreateTapFile();
        var original = InfoCommand.GetFileInfo(inputFile.Path, inputFile.Bytes);
        var originalItems = original.Sections[0].Items;

        var json = JsonSerializer.Serialize(original, FileInfoJsonContext.Default.FileInfoResult);
        var deserialized = JsonSerializer.Deserialize(json, FileInfoJsonContext.Default.FileInfoResult);

        deserialized.Should().NotBeNull();
        var deserializedItems = deserialized!.Sections[0].Items;
        deserializedItems.Count.Should().Equal(originalItems.Count);
        for (var i = 0; i < originalItems.Count; i++)
        {
            deserializedItems[i].Title.Should().Equal(originalItems[i].Title);
            deserializedItems[i].Properties.Count.Should().Equal(originalItems[i].Properties.Count);
        }
    }

    [Test]
    public void RoundTrip_UsesCamelCase()
    {
        using var inputFile = CreateTapFile();
        var json = InfoCommand.GetFileInfoJson(inputFile.Path, inputFile.Bytes);

        json.Should().Contain("\"format\":");
        json.Should().Contain("\"fileExtension\":");
        json.Should().Contain("\"convertibleTo\":");
        json.Should().Contain("\"sections\":");
    }

    [Test]
    public void RoundTrip_NullFormatOmittedInJson()
    {
        // The JsonIgnoreCondition.WhenWritingNull option should omit null format values.
        using var inputFile = CreateTapFile();
        var json = InfoCommand.GetFileInfoJson(inputFile.Path, inputFile.Bytes);

        // "Type" property on TAP header block items should not have a format, so "format" key should be absent for it.
        // But "format":"hex" should appear for hex-formatted properties.
        // Verify the JSON doesn't contain "format":null.
        json.Should().NotContain("\"format\":null");
    }

    [Test]
    public void RoundTrip_PreservesFormatHints()
    {
        using var inputFile = CreateZ80File();
        var original = InfoCommand.GetFileInfo(inputFile.Path, inputFile.Bytes);

        var json = JsonSerializer.Serialize(original, FileInfoJsonContext.Default.FileInfoResult);
        var deserialized = JsonSerializer.Deserialize(json, FileInfoJsonContext.Default.FileInfoResult);

        deserialized.Should().NotBeNull();

        // Z80 registers should have hex format hints.
        var registersSection = deserialized!.Sections.Single(s => s.Title == "Registers");
        registersSection.Properties[0].Format.Should().Equal("hex");
    }
}