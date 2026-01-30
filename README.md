# Label Nesting

<p align="center">
  <img src="logo.svg" width="200" alt="Label Nesting Logo">
</p>

An efficient label packing application that arranges rectangular labels on paper sheets to minimize waste.

## Overview

Label Nesting uses advanced packing algorithms to optimize the placement of multiple labels on standard paper sizes, helping you reduce material costs and maximize paper utilization.

## Projects

- **Shunty.LabelNesting.Core** - Core packing algorithms and models
- **Shunty.LabelNesting.Web** - Web-based user interface
- **Shunty.LabelNesting.Cli** - Command-line interface
- **Shunty.LabelNesting.AppHost** - .NET Aspire orchestration

## Quick Start

### Prerequisites

- .NET 10.0 SDK
- Docker (optional, for containerized deployment)

### Build

```bash
dotnet build
```

### Run Web Application

```bash
dotnet run --project src/Shunty.LabelNesting.AppHost
```

Or using the watch task:
```bash
dotnet watch run --project src/Shunty.LabelNesting.Web
```

### Run CLI

```bash
dotnet run --project src/Shunty.LabelNesting.Cli
```

### Run Tests

```bash
dotnet test
```

## Docker

Build and run using Docker:

```bash
docker build -t label-nesting .
docker-compose up
```

## Documentation

Further documentation is available in the `./docs` folder.

## License

This project is licensed under the "do what you want with it" license. It comes with no warranties, guarantees or promises of any kind whatsoever.
