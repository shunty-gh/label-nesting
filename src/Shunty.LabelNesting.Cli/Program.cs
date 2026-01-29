using Shunty.LabelNesting.Cli.Commands;
using Spectre.Console.Cli;

var app = new CommandApp<PackCommand>();

app.Configure(config =>
{
    config.SetApplicationName("labelnest");
    config.SetApplicationVersion("1.0.0");

    config.AddCommand<PackCommand>("pack")
        .WithDescription("Pack items onto paper and generate a PDF")
        .WithExample("pack", "-p", "A4", "-i", "100,50,3", "-i", "75,25,5", "-o", "output.pdf")
        .WithExample("pack", "--interactive")
        .WithExample("pack", "-p", "200x300", "-i", "50,50,10", "--no-rotation");
});

return app.Run(args);
