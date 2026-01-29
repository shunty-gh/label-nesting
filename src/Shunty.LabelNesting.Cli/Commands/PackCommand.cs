using Shunty.LabelNesting.Cli.Interactive;
using Shunty.LabelNesting.Core.Algorithms;
using Shunty.LabelNesting.Core.Models;
using Shunty.LabelNesting.Core.Services;
using Shunty.LabelNesting.Core.Validation;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shunty.LabelNesting.Cli.Commands;

/// <summary>
/// Command to pack items onto paper and generate a PDF.
/// </summary>
public sealed class PackCommand : Command<PackCommandSettings>
{
    public override int Execute(CommandContext context, PackCommandSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (settings.Interactive)
            {
                return RunInteractive();
            }

            return RunBatch(settings);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }

    private static int RunInteractive()
    {
        AnsiConsole.Write(new FigletText("Label Nesting").Color(Color.Green));
        AnsiConsole.MarkupLine("[dim]Optimize rectangular item placement on paper[/]\n");

        var session = new InteractiveSession();

        var paperSize = session.PromptPaperSize();
        AnsiConsole.MarkupLine($"[dim]Selected paper: {paperSize}[/]");

        var config = session.PromptConfiguration();
        var items = session.PromptItems(paperSize, config);
        var outputPath = session.PromptOutputPath();

        return ExecutePacking(items, paperSize, config, outputPath);
    }

    private static int RunBatch(PackCommandSettings settings)
    {
        // Parse paper size
        if (!PaperSize.TryParse(settings.PaperSize, out var paperSize))
        {
            AnsiConsole.MarkupLine($"[red]Invalid paper size: {settings.PaperSize}[/]");
            AnsiConsole.MarkupLine("[dim]Use A2-A6 or custom format like 200x300[/]");
            return 1;
        }

        // Parse items
        if (settings.Items is null || settings.Items.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]No items specified. Use -i option or --interactive mode.[/]");
            AnsiConsole.MarkupLine("[dim]Example: labelnest pack -p A4 -i 100,50,3 -i 75,25,5 -o output.pdf[/]");
            return 1;
        }

        var items = new List<Item>();
        foreach (var itemSpec in settings.Items)
        {
            var (isValid, error) = ItemValidator.ValidateItemString(itemSpec);
            if (!isValid)
            {
                AnsiConsole.MarkupLine($"[red]Invalid item '{itemSpec}': {error}[/]");
                return 1;
            }
            items.Add(Item.Parse(itemSpec));
        }

        var config = new PackingConfiguration(
            Margin: settings.Margin,
            Gutter: settings.Gutter,
            AllowRotation: !settings.NoRotation);

        return ExecutePacking(items, paperSize, config, settings.OutputPath);
    }

    private static int ExecutePacking(
        List<Item> items,
        PaperSize paperSize,
        PackingConfiguration config,
        string outputPath)
    {
        // Validate items fit
        var errors = ItemValidator.ValidateItems(items, paperSize, config);
        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                AnsiConsole.MarkupLine($"[red]{error}[/]");
            }
            return 1;
        }

        // Pack items
        var colorProvider = new RandomColorProvider();
        var algorithm = new MaxRectsAlgorithm(colorProvider);

        AnsiConsole.Status()
            .Start("Packing items...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
            });

        var result = algorithm.Pack(items, paperSize, config);

        // Display results table
        DisplayResults(result, items);

        // Generate PDF
        AnsiConsole.Status()
            .Start("Generating PDF...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                var pdfGenerator = new PdfGenerator();
                pdfGenerator.Generate(result, outputPath);
            });

        AnsiConsole.MarkupLine($"\n[green]PDF saved to: {outputPath}[/]");

        return 0;
    }

    private static void DisplayResults(PackingResult result, List<Item> items)
    {
        AnsiConsole.WriteLine();

        // Summary panel
        var panel = new Panel(
            $"Pages: [green]{result.PageCount}[/] | " +
            $"Items placed: [green]{result.TotalItemsPlaced}[/] | " +
            $"Efficiency: [green]{result.OverallEfficiency:P1}[/]")
        {
            Header = new PanelHeader("Packing Summary"),
            Border = BoxBorder.Rounded
        };
        AnsiConsole.Write(panel);

        // Items table
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Item")
            .AddColumn("Size (mm)")
            .AddColumn("Qty")
            .AddColumn("Placed");

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var placedCount = result.Placements.Count(p => p.ItemIndex == i);
            table.AddRow(
                $"{i + 1}",
                $"{item.Width} x {item.Height}",
                item.Quantity.ToString(),
                placedCount == item.Quantity ? $"[green]{placedCount}[/]" : $"[yellow]{placedCount}/{item.Quantity}[/]");
        }

        AnsiConsole.Write(table);

        // Page efficiency breakdown
        if (result.PageCount > 1)
        {
            AnsiConsole.WriteLine();
            var pageTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Page")
                .AddColumn("Items")
                .AddColumn("Efficiency");

            for (var i = 0; i < result.PageCount; i++)
            {
                var pageItems = result.GetPagePlacements(i).Count();
                var efficiency = result.GetPageEfficiency(i);
                pageTable.AddRow(
                    $"{i + 1}",
                    pageItems.ToString(),
                    $"{efficiency:P1}");
            }

            AnsiConsole.Write(pageTable);
        }
    }
}
