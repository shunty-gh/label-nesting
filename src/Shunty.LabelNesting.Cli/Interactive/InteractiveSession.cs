using Shunty.LabelNesting.Core.Models;
using Shunty.LabelNesting.Core.Validation;
using Spectre.Console;

namespace Shunty.LabelNesting.Cli.Interactive;

/// <summary>
/// Handles interactive prompts for gathering packing parameters.
/// </summary>
public sealed class InteractiveSession
{
    public PaperSize PromptPaperSize()
    {
        var choices = PaperSize.StandardSizes
            .Select(s => $"{s.Name} ({s.Width}x{s.Height}mm)")
            .Append("Custom")
            .ToArray();

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select [green]paper size[/]:")
                .AddChoices(choices));

        if (selection == "Custom")
        {
            var width = AnsiConsole.Prompt(
                new TextPrompt<double>("Enter [green]width[/] in mm:")
                    .Validate(w => w > 0 ? ValidationResult.Success() : ValidationResult.Error("Width must be positive")));

            var height = AnsiConsole.Prompt(
                new TextPrompt<double>("Enter [green]height[/] in mm:")
                    .Validate(h => h > 0 ? ValidationResult.Success() : ValidationResult.Error("Height must be positive")));

            return PaperSize.Custom(width, height);
        }

        var index = Array.IndexOf(choices, selection);
        return PaperSize.StandardSizes[index];
    }

    public List<Item> PromptItems(PaperSize paperSize, PackingConfiguration config)
    {
        var items = new List<Item>();

        AnsiConsole.MarkupLine("\n[yellow]Enter items (width,height,quantity). Leave empty to finish.[/]");
        AnsiConsole.MarkupLine("[dim]Example: 100,50,3 (100mm x 50mm, quantity 3)[/]\n");

        while (true)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>($"[green]Item {items.Count + 1}[/]:")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(input))
            {
                if (items.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]At least one item is required.[/]");
                    continue;
                }
                break;
            }

            var (isValid, error) = ItemValidator.ValidateItemString(input);
            if (!isValid)
            {
                AnsiConsole.MarkupLine($"[red]{error}[/]");
                continue;
            }

            var item = Item.Parse(input);

            // Check if item fits on paper
            if (!config.CanFit(item, paperSize))
            {
                var usableWidth = config.GetUsableWidth(paperSize);
                var usableHeight = config.GetUsableHeight(paperSize);
                AnsiConsole.MarkupLine(
                    $"[red]Item {item.Width}x{item.Height}mm is too large for {paperSize}. " +
                    $"Usable area: {usableWidth}x{usableHeight}mm.[/]");
                continue;
            }

            items.Add(item);
            AnsiConsole.MarkupLine($"[dim]Added: {item.Width}x{item.Height}mm x {item.Quantity}[/]");
        }

        return items;
    }

    public PackingConfiguration PromptConfiguration()
    {
        var useDefaults = AnsiConsole.Confirm("Use default configuration (5mm margin, 2mm gutter, rotation enabled)?", true);

        if (useDefaults)
        {
            return PackingConfiguration.Default;
        }

        var margin = AnsiConsole.Prompt(
            new TextPrompt<double>("Enter [green]margin[/] in mm:")
                .DefaultValue(5.0)
                .Validate(m => m >= 0 ? ValidationResult.Success() : ValidationResult.Error("Margin cannot be negative")));

        var gutter = AnsiConsole.Prompt(
            new TextPrompt<double>("Enter [green]gutter[/] (space between items) in mm:")
                .DefaultValue(2.0)
                .Validate(g => g >= 0 ? ValidationResult.Success() : ValidationResult.Error("Gutter cannot be negative")));

        var allowRotation = AnsiConsole.Confirm("Allow item rotation?", true);

        return new PackingConfiguration(margin, gutter, allowRotation);
    }

    public string PromptOutputPath()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("Enter [green]output PDF path[/]:")
                .DefaultValue("output.pdf"));
    }
}
