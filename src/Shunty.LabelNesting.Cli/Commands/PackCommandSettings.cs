using System.ComponentModel;
using Spectre.Console.Cli;

namespace Shunty.LabelNesting.Cli.Commands;

/// <summary>
/// Settings for the pack command.
/// </summary>
public sealed class PackCommandSettings : CommandSettings
{
    [CommandOption("-p|--paper <SIZE>")]
    [Description("Paper size (A2-A6 or custom like 200x300). Default: A4")]
    [DefaultValue("A4")]
    public string PaperSize { get; init; } = "A4";

    [CommandOption("-o|--output <FILE>")]
    [Description("Output PDF file path. Default: output.pdf")]
    [DefaultValue("output.pdf")]
    public string OutputPath { get; init; } = "output.pdf";

    [CommandOption("-i|--item <SPEC>")]
    [Description("Item specification: width,height or width,height,quantity. Can be specified multiple times.")]
    public string[]? Items { get; init; }

    [CommandOption("-m|--margin <MM>")]
    [Description("Page margin in millimeters. Default: 5")]
    [DefaultValue(5.0)]
    public double Margin { get; init; } = 5.0;

    [CommandOption("-g|--gutter <MM>")]
    [Description("Space between items in millimeters. Default: 2")]
    [DefaultValue(2.0)]
    public double Gutter { get; init; } = 2.0;

    [CommandOption("--no-rotation")]
    [Description("Disable item rotation")]
    [DefaultValue(false)]
    public bool NoRotation { get; init; }

    [CommandOption("--interactive")]
    [Description("Run in interactive mode with prompts")]
    [DefaultValue(false)]
    public bool Interactive { get; init; }
}
