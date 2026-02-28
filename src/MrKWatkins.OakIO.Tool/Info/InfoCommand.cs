using Spectre.Console.Cli;

namespace MrKWatkins.OakIO.Tool.Info;

[UsedImplicitly]
internal sealed class InfoCommand : Command<InfoSettings>
{
    public override int Execute(CommandContext context, InfoSettings settings, CancellationToken cancellationToken)
    {
        using var inputStream = File.OpenRead(settings.Input);
        Commands.InfoCommand.Execute(settings.Input, inputStream, Console.Out);
        return 0;
    }
}