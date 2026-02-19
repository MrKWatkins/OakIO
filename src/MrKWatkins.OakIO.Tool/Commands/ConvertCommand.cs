using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrKWatkins.OakIO.Tool.Commands;

internal sealed class ConvertCommand : Command<ConvertCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Path to the input file.")]
        public required string Input { get; init; }

        [CommandArgument(1, "<output>")]
        [Description("Path to the output file.")]
        public required string Output { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        using var inputStream = File.OpenRead(settings.Input);
        using var outputStream = File.Create(settings.Output);
        MrKWatkins.OakIO.Commands.ConvertCommand.Execute(settings.Input, inputStream, settings.Output, outputStream);
        AnsiConsole.MarkupLine($"Converted [green]{settings.Input}[/] to [green]{settings.Output}[/].");
        return 0;
    }
}
