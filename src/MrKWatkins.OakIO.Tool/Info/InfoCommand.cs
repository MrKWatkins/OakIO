using Spectre.Console.Cli;

namespace MrKWatkins.OakIO.Tool.Info;

[UsedImplicitly]
internal sealed class InfoCommand : AsyncCommand<InfoSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, InfoSettings settings, CancellationToken cancellationToken)
    {
        var inputStream = File.OpenRead(settings.Input);
        await using (inputStream.ConfigureAwait(false))
        {
            await Commands.InfoCommand.ExecuteAsync(settings.Input, inputStream, Console.Out, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        return 0;
    }
}