using MrKWatkins.OakIO.Tool;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(OakIOTool.Configure);
return app.Run(args);