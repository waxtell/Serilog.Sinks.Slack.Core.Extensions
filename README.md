# Serilog.Sinks.Slack.Core.Extensions
Verbose renderer extension for the Serilog.Sinks.Slack.Core package.

```
{  
  "Serilog": {
    "Using": [ "Serilog.Enrichers.AssemblyName", "Serilog.Sinks.Slack.Core.Extensions" ],
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      {
        "Name": "VerboseSlack",
        "Args": {
          "webhookUri": "your webhook url goes here",
          "restrictedToMinimumLevel": "Error"
        }
      }
    ],
    "Enrich": [ "WithAssemblyName", "WithAssemblyVersion" ]
  }
}  
  ```