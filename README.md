![NuGet Version](https://img.shields.io/nuget/v/Vestfold.Extensions.Logging.svg)
![NuGet Downloads](https://img.shields.io/nuget/dt/Vestfold.Extensions.Logging.svg)

# Vestfold.Extensions.Logging

Contains builder extensions for configuring logging in a dotnet core application.

## Usage in an Azure Function / Azure Web App

> [!IMPORTANT]
> Azure App Services does not allow periods (.) in the app setting names.<br />
> As a workaround, use an underscore (_) instead of a period (.) in the app setting names, and it will be handled correctly in the code.<br />
> If an app setting contains a period, the period is replaced with an underscore (_) in the container.

Add the following to your `local.settings.json` file:

All properties (except `AzureWebJobsStorage` and `FUNCTIONS_WORKER_RUNTIME` which is Azure specific) are optional.

> Semi optional properties:
> - `AppName`: If not set, the assembly name will be used as AppName property in logs
> - `Version`: If not set, the assembly version will be used as Version property in logs

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
    "MicrosoftTeams_WebhookUrl": "microsoft teams webhook url | microsoft power automate flow url if UseWorkflows is set to true",
    "MicrosoftTeams_UseWorkflows": "true if Microsoft Power Automate flow is used, false if Microsoft Teams webhook is used (default is true)",
    "MicrosoftTeams_TitleTemplate": "The title template of the card",
    "MicrosoftTeams_MinimumLevel": "Warning",
    "Version": "1.0.0"
  }
}
```

### Override minimum levels for certain namespaces

> [!IMPORTANT]
> If you want to override a namespace which contains a period (.), for instance `Microsoft.Hosting`,
> you need to use an underscore (_) since Azure App Services does not allow periods (.) in the app setting names.

Example: Add an override for everything in the `Microsoft.Hosting` namespace to log from **Warning** and higher
```json
{
  "Values": {
    "Serilog_MinimumLevel_Override_Microsoft_Hosting": "Warning"
  }
}
```

## Usage outside Azure

Add the following to your `appsettings.json` file:

All properties are optional.

> Semi optional properties:
> - `AppName`: If not set, the assembly name will be used as AppName property in logs
> - `Version`: If not set, the assembly version will be used as Version property in logs

```json
{
  "AppName": "Name of your application, used as a property in the logs",
  "BetterStack": {
    "SourceToken": "Your BetterStack source token",
    "Endpoint": "https://foo.betterstackdata.com",
    "MinimumLevel": "Information"
  },
  "MicrosoftTeams": {
    "WebhookUrl": "microsoft teams webhook url | microsoft power automate flow url if UseWorkflows is set to true",
    "UseWorkflows": "true if Microsoft Power Automate flow is used, false if Microsoft Teams webhook is used (default is true)",
    "TitleTemplate": "The title template of the card",
    "MinimumLevel": "Warning"
  },
  "Version": "1.0.0"
}
```

### Override minimum levels for certain namespaces

Example: Add an override for everything in the `Microsoft.Hosting` namespace to log from **Warning** and higher
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "Microsoft.Hosting": "Warning"
      }
    }
  }
}
```

## Setting up logging for an Azure Function / Azure Web App

```csharp
var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.Logging.AddVestfoldLogging();
builder.Build().Run();
```

## Setting up logging for a HostBuilder (Console app, ClassLibrary, etc.)

```csharp
public static async Task Main(string[] args)
{
    await Host.CreateDefaultBuilder(args)
        .ConfigureLogging(builder => builder.AddVestfoldLogging())
        .Build()
        .RunAsync();

    await Serilog.Log.CloseAndFlushAsync();
}
```

## Setting up logging for a WebApplicationBuilder (WebAPI, Blazor, etc.)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddVestfoldLogging();

var app = builder.Build();
```