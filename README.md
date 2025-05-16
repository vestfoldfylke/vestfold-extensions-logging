![NuGet Version](https://img.shields.io/nuget/v/VFK.Extensions.Logging.svg)
![NuGet Downloads](https://img.shields.io/nuget/dt/VFK.Extensions.Logging.svg)

# VFK.Extensions.Logging

Contains builder extensions for configuring logging in a dotnet core application.

## Usage in an Azure Function / Azure Web App

Add the following to your `local.settings.json` file:

> Optional properties:
> - `AppName`: If not set, the assembly name will be used
> - `Version`: If not set, the assembly version will be used

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AppName": "Name of your application, used as a property in the logs",
    "BetterStack_SourceToken": "Your BetterStack source token",
    "BetterStack_Endpoint": "https://foo.betterstackdata.com",
    "BetterStack_MinimumLevel": "Information",
    "Version": "1.0.0"
  }
}
```

### Override minimum levels for certain namespaces

Example: Add an override for everything in the `Microsoft` namespace to log from **Warning** and higher
```json
{
  "Values": {
    "Serilog_MinimumLevel_Override_Microsoft": "Warning"
  }
}
```

## Usage outside Azure

Add the following to your `appsettings.json` file:

> Optional properties:
> - `AppName`: If not set, the assembly name will be used
> - `Version`: If not set, the assembly version will be used

```json
{
  "AppName": "Name of your application, used as a property in the logs",
  "BetterStack": {
    "SourceToken": "Your BetterStack source token",
    "Endpoint": "https://foo.betterstackdata.com",
    "MinimumLevel": "Information"
  },
  "Version": "1.0.0"
}
```

### Override minimum levels for certain namespaces

Example: Add an override for everything in the `Microsoft` namespace to log from **Warning** and higher
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "Microsoft": "Warning"
      }
    }
  }
}
```

## Setting up logging for an Azure Function / Azure Web App

```csharp
var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.Logging.AddVfkLogging();
builder.Build().Run();
```

## Setting up logging for a HostBuilder (Console app, ClassLibrary, etc.)

```csharp
public static async Task Main(string[] args)
{
    await Host.CreateDefaultBuilder(args)
        .ConfigureLogging(builder => builder.AddVfkLogging())
        .Build()
        .RunAsync();

    await Serilog.Log.CloseAndFlushAsync();
}
```

## Setting up logging for a WebApplicationBuilder (WebAPI, Blazor, etc.)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddVfkLogging();

var app = builder.Build();
```