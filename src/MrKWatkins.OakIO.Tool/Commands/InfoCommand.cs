using System.ComponentModel;
using Spectre.Console.Cli;

namespace MrKWatkins.OakIO.Tool.Commands;

internal sealed class InfoCommand : Command<InfoCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Path to the input file.")]
        public required string Input { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        using var inputStream = File.OpenRead(settings.Input);
        MrKWatkins.OakIO.Commands.InfoCommand.Execute(settings.Input, inputStream, Console.Out);
        return 0;
    }
}