using MrKWatkins.OakIO.Tool.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("oakio");
    config.AddCommand<InfoCommand>("info")
        .WithDescription("Display information about a file.");
    config.AddCommand<ConvertCommand>("convert")
        .WithDescription("Convert a file from one format to another.");
});

return app.Run(args);
