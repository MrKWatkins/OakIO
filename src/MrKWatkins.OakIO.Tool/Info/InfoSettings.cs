using System.ComponentModel;
using Spectre.Console.Cli;

namespace MrKWatkins.OakIO.Tool.Info;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class InfoSettings : CommandSettings
{
    [CommandArgument(0, "<input>")]
    [Description("Path to the input file.")]
    public required string Input { get; init; }
}