# Label Nesting - Copilot Instructions

## Project Overview

Label Nesting is a .NET 10.0 application that provides efficient label packing algorithms to arrange rectangular labels on paper sheets with minimal waste. The project consists of multiple components including a core library, web interface, CLI tool, and .NET Aspire orchestration.

## Technology Stack

- **Framework**: .NET 10.0 (net10.0)
- **Language**: C# 13.0
- **Architecture**: Multi-project solution using Central Package Management
- **Key Dependencies**:
  - QuestPDF for PDF generation
  - SkiaSharp for graphics
  - Spectre.Console for CLI
  - .NET Aspire for orchestration
  - MSTest for testing
  - Playwright for E2E testing

## Projects Structure

- **Shunty.LabelNesting.Core** - Core packing algorithms and models
- **Shunty.LabelNesting.Web** - Web-based user interface
- **Shunty.LabelNesting.Cli** - Command-line interface
- **Shunty.LabelNesting.AppHost** - .NET Aspire orchestration
- **Shunty.LabelNesting.ServiceDefaults** - Shared service configuration
- **Shunty.LabelNesting.Tests** - Test project

## Build and Test Commands

### Build the project
```bash
dotnet build
```

### Run tests
```bash
dotnet test
```

### Run the web application
```bash
dotnet run --project src/Shunty.LabelNesting.AppHost
```

Or with watch mode:
```bash
dotnet watch run --project src/Shunty.LabelNesting.Web
```

### Run the CLI
```bash
dotnet run --project src/Shunty.LabelNesting.Cli
```

## Coding Standards and Conventions

### General C# Standards

- **Nullable Reference Types**: Always enabled - use nullable annotations appropriately
- **Implicit Usings**: Enabled - common namespaces are imported automatically
- **Language Version**: C# 13.0 - use latest language features where appropriate
- **Warnings as Errors**: All warnings are treated as errors - code must be warning-free
- **Code Style Enforcement**: Code style is enforced during build

### Naming Conventions

- Use PascalCase for public members, classes, and methods
- Use camelCase for private fields with leading underscore (_field)
- Use meaningful, descriptive names

### Code Organization

- One class per file
- File-scoped namespaces
- Organize using statements

### Testing Standards

- **Framework**: MSTest (Microsoft.VisualStudio.TestTools.UnitTesting)
- **Test Class**: Mark with `[TestClass]` and use `sealed` modifier
- **Test Methods**: Mark with `[TestMethod]`
- **Setup**: Use `[TestInitialize]` for test setup
- **Assertions**: Use MSTest Assert methods (Assert.AreEqual, Assert.IsTrue, etc.)
- **Naming**: Use descriptive test names in the format `MethodName_Scenario_ExpectedBehavior`
- **Test Organization**: Group tests by functionality in separate files

### Package Management

- Use Central Package Management (Directory.Packages.props)
- All package versions are defined centrally
- When adding new packages, add version to Directory.Packages.props

### Dependencies

- Prefer using existing libraries in the solution
- Only add new dependencies when absolutely necessary
- All new dependencies must be added to Directory.Packages.props

## What NOT to Change

- Do not modify `Directory.Build.props` without explicit requirement
- Do not change `global.json` SDK version without explicit requirement
- Do not modify Central Package Management configuration
- Do not disable nullable reference types
- Do not disable warnings as errors
- Do not add dependencies without considering security implications

## Development Workflow

1. Build the project first to ensure it compiles
2. Run tests to ensure existing functionality works
3. Make minimal, focused changes
4. Test changes thoroughly
5. Ensure no warnings or errors

## Documentation

- Keep README.md up to date
- Document public APIs and complex algorithms
- Add XML documentation comments for public types and members

## Security

- Never commit secrets or sensitive data
- Validate all user inputs
- Follow secure coding practices
- Be mindful of package vulnerabilities
