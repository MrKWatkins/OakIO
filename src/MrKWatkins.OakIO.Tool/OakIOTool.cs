using MrKWatkins.OakIO.Tool.Convert;
using MrKWatkins.OakIO.Tool.Info;
using Spectre.Console.Cli;

namespace MrKWatkins.OakIO.Tool;

public static class OakIOTool
{
    public static void Configure(IConfigurator config)
    {
        config.SetApplicationName("oakio");
        config.AddCommand<InfoCommand>("info").WithDescription("Display information about a file.");
        config.AddCommand<ConvertCommand>("convert").WithDescription("Convert a file from one format to another.");
    }
}