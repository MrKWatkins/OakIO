using Spectre.Console;
using Spectre.Console.Cli;

namespace MrKWatkins.OakIO.Tool.Convert;

[UsedImplicitly]
internal sealed class ConvertCommand : Command<ConvertSettings>
{
    public override int Execute(CommandContext context, ConvertSettings settings, CancellationToken cancellationToken)
    {
        using var inputStream = File.OpenRead(settings.Input);
        using var outputStream = File.Create(settings.Output);
        Commands.ConvertCommand.Execute(settings.Input, inputStream, settings.Output, outputStream);
        AnsiConsole.MarkupLine($"Converted [green]{settings.Input}[/] to [green]{settings.Output}[/].");
        return 0;
    }
}