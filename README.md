![NuGet Version](https://img.shields.io/nuget/v/Vestfold.Extensions.Logging.svg)
![NuGet Downloads](https://img.shields.io/nuget/dt/Vestfold.Extensions.Logging.svg)

# Vestfold.Extensions.Logging

Contains builder extensions for configuring logging in a dotnet core application.

## Adding environment variables in an Azure App Service (`Web App` or `Function App`)
> [!IMPORTANT]
> `Azure App Services` (actually it's Linux) does not allow periods (.) in the app setting names.<br />
> If an app setting contains a period (.), the period is replaced with an underscore (\_) in the container automatically by `Azure App Services`.<br />
> As a workaround, use an underscore (\_) instead of a period (.) in the app setting names, and it will be handled correctly in the code.

> [!IMPORTANT]
> `Azure App Services` (actually it's Linux) does not allow colons (:) in the app setting names, which are typically used in configuration names to denote nested configuration sections (e.g., `Serilog:MinimumLevel:Override:Microsoft`).<br />
> As a workaround, use double underscores (\_\_) instead of a colon (:) in the app, and the dotnet runtime will automatically translate the double underscores into colons when building the configuration, allowing you to maintain the hierarchical structure of your configuration settings without any issues.<br />
> Example: `Serilog__MinimumLevel__Override__Microsoft` will be translated to `Serilog:MinimumLevel:Override:Microsoft` in the configuration.

## Usage in an `Azure Function App`

Add the following to your `local.settings.json` file to have a nested structure for the configuration:

All properties (except `AzureWebJobsStorage` (`Azure App Service` specific) and `FUNCTIONS_WORKER_RUNTIME` (`Azure Function App` specific) are optional.

> Semi optional properties:
> - `AppName`: If not set, the assembly name will be used as AppName property in logs<br />
> - `AppVersion`: If not set, the assembly version will be used as Version property in logs.<br />
> [!CAUTION]: In Azure Functions, the `Azure App Services` automatically injects an environment variable named `Version` with the version of the Azure Functions runtime, which will override the assembly version IF `AppVersion` is not set. To avoid this, it's recommended to explicitly set the `AppVersion` property in Azure Functions to ensure the correct version is used in the logs.

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AppName": "Name of your application, used as a property in the logs",
    "AppVersion": "1.0.0",
    "BetterStack__SourceToken": "Your BetterStack source token",
    "BetterStack__Endpoint": "https://foo.betterstackdata.com",
    "BetterStack__MinimumLevel": "Information",
    "MicrosoftTeams__WebhookUrl": "microsoft teams webhook url | microsoft power automate flow url if UseWorkflows is set to true",
    "MicrosoftTeams__UseWorkflows": "true if Microsoft Power Automate flow is used, false if Microsoft Teams webhook is used (default is true)",
    "MicrosoftTeams__TitleTemplate": "The title template of the card",
    "MicrosoftTeams__MinimumLevel": "Warning",
    "Serilog__Console__MinimumLevel": "Information",
    "Serilog__File__Path": "log.txt",
    "Serilog__File__MinimumLevel": "Warning",
    "Serilog__File__RollingInterval": "Day"
  }
}
```

### Override minimum levels for certain namespaces

> [!IMPORTANT]
> If you want to override a namespace which contains a period (.), for instance `Microsoft.Hosting`,<br />
> you need to use an underscore (\_) since `Azure App Services` (actually it's Linux) does not allow periods (.) in the app setting names.

Example: Add an override for everything in the `Microsoft.Hosting` namespace to log from **Warning** and higher
```json
{
  "Values": {
    "Serilog__MinimumLevel__Override__Microsoft_Hosting": "Warning"
  }
}
```

## Usage outside Azure

Add the following to your `appsettings.json` file:

All properties are optional.

> Semi optional properties:
> - `AppName`: If not set, the assembly name will be used as AppName property in logs<br />
> - `AppVersion`: If not set, the assembly version will be used as Version property in logs

```json
{
  "AppName": "Name of your application, used as a property in the logs",
  "AppVersion": "1.0.0",
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
  "Serilog": {
    "Console": {
      "MinimumLevel": "Information"
    },
    "File": {
      "Path": "log.txt",
      "MinimumLevel": "Warning",
      "RollingInterval": "Day"
    }
  }
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

## Setting up logging for an `Azure Function App` / `Azure Web App`

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
