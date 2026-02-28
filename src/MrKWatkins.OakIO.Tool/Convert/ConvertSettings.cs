using System.ComponentModel;
using Spectre.Console.Cli;

namespace MrKWatkins.OakIO.Tool.Convert;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class ConvertSettings : CommandSettings
{
    [CommandArgument(0, "<input>")]
    [Description("Path to the input file.")]
    public required string Input { get; init; }

    [CommandArgument(1, "<output>")]
    [Description("Path to the output file.")]
    public required string Output { get; init; }
}