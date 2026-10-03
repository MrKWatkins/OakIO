using Spectre.Console;
using Spectre.Console.Cli;

namespace MrKWatkins.OakIO.Tool.Convert;

[UsedImplicitly]
internal sealed class ConvertCommand : AsyncCommand<ConvertSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ConvertSettings settings, CancellationToken cancellationToken)
    {
        var inputStream = File.OpenRead(settings.Input);
        await using (inputStream.ConfigureAwait(false))
        {
            var outputStream = File.Create(settings.Output);
            await using (outputStream.ConfigureAwait(false))
            {
                await Commands.ConvertCommand.ExecuteAsync(settings.Input, inputStream, settings.Output, outputStream, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }
        AnsiConsole.MarkupLine($"Converted [green]{settings.Input}[/] to [green]{settings.Output}[/].");
        return 0;
    }
}