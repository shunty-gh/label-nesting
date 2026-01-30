# Description

This is the brief specification for an application that takes, as input, a list of quantities+dimensions for print documents along with a target size of paper and then produce, as output, a PDF layout that minimizes paper waste when printing all items on the given size of paper. 

# Use Case

An end user (for example a printer or a retail store) will have a number of print items - mostly, but not limited to, PDF, JPG or PNG formats. The items will be of varying dimensions but will always be regular rectangles. 
The user will want to print a selection of them (any number of any of them) and will want to minimise the amount of paper required so will need to be able to arrange the items on each page in the most space saving manner.
They will require the software to produce a suitable layout description and a PDF (possibly multi-page) showing the layout using plain coloured boxes to represent each item.

# Input and output

- Input items should be supplied as width (in mm), height (in mm), quantity (+ve integer)
- Target paper size should be one of 'A2', 'A3', 'A4', 'A5', 'A6' or a custom size given as (width, height) in mm

- Output should be a PDF (of 1..n pages) showing the layout and a list showing the top,left co-ordinate on the output for each item in the input

I am thinking that maybe the program should take the input and then delegate to an LLM model, in the long run, but for now we should try and calculate the layout in code using known techinques.

# Tech Stack And Structure

- The application should be written in modern C#, dotnet 10, using up to date techniques and features.
- For ease of development and debugging we should also use Aspire to orchestrate the application.
- The application should be structured as:
    - Shunty.LabelNesting.Core    
        The core class library where most of the non-ui code will be held
    - Shunty.LabelNesting.Cli
        A console application, using Spectre Console (or similar) that allows the user to enter, interactively from the console, the required inputs that the app will then pass to the core for processing and then the app will save the output to a disk file (name specified by the user). This console application should also be able to accept and process all inputs and parameters from the command line.
    - Shunty.LabelNesting.Web
        A Blazor server based application that provides a web based front end for interactive input of the data and on-screen visualisation of the output - allowing viewing, download, etc
    - Shunty.LabelNesting.Test
        A test suite for the application
    - Shunty.Labelnesting.ServiceDefaults
        The standard Aspire service defaults project
    - Shunty.LabelNesting.AppHost
        The standard Aspire app host project

- Logging should be output to both console and OTEL/OTLP endpoint(s)
- Tests should use MSTest
- We should use the Microsoft Learn mcp server for up to date dotnet documentation
- We should use the Playwright mcp server to help produce tests. Specifically E2E tests.
- Once built, we should add suitable `launch.json`, `tasks.json` and `mcp.json` files to enable VS Code to run and debug the project. The Aspire apphost should be the startup project.

# Notes

- The items may be rotated to allow for better fit
- We will need to allow for user definable margins between each item (default to 2mm) and non-printable areas around the page (default to 5mm)
- It is up to the AI to pick an appropriate packing method. I have no experience of the existing methods. It may be appropriate to offer the user a choice between two methods - eg one for performance and one for best fit.
- Oversized items can be determined before processing. For now, if the user includes an oversized item then we should just fail the entire process and let the user know why.
- Sequential numbering and random colouring of the items will be fine for output purposes.
- It is currently undecided on which PDF library to use. The AI should pick an appropriate one based on pouplarity and price. Free is best, free trial is second best. Ease of swapping out should be considered too.
- Batch input of items can be added later. It will be required at some point.
- The Blazor app only needs the ability to view the output for now. Interactivity and repositioning may be nice in a future revision once everythin else is working correctly.
