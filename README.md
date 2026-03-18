# NaptimeDaemon

`NaptimeDaemon` is a lightweight Windows tray app that monitors system idle time and can put your PC to sleep when your configured threshold is reached.

## Features

- Runs in the Windows notification tray
- Single-instance app guard (prevents duplicate running copies)
- Live main window status:
  - `Sleep In` countdown
  - Current idle threshold
- In-app settings window:
  - Edit idle threshold minutes
  - Quick presets (`5 min`, `60 min`)
- Optional manual actions:
  - `Sleep Now`
  - `View Log` (wake diagnostics)
- Wake diagnostics logging on startup (`powercfg` snapshots)

## Solution Structure

- `NaptimeDaemon` - WPF tray app (UI + app orchestration)
- `NaptimeDaemon.Core` - policy/config model logic
- `Naptime.Daemon.Platform.Windows` - Windows platform integrations (idle time, sleep)
- `NaptimeDaemon.Core.Tests` - core policy tests

## Requirements

- Windows
- .NET SDK `10.0`+

## Run from source

```powershell
dotnet build .\NaptimeDaemon.slnx
dotnet run --project .\NaptimeDaemon\NaptimeDaemon.App.csproj
```

## Configuration

Config file location:

- `%AppData%\NaptimeDaemon\config.json`

Current app flow:

- App loads config on startup
- Policy agent starts with the loaded config
- Saving config in Settings restarts the policy agent with new values

## Wake diagnostics log

Wake log location:

- `%AppData%\NaptimeDaemon\wake-log.txt`

Generated at startup and viewable from:

- Tray menu: `View Wake Log`
- Main window button: `View Log`

## Publish (self-contained EXE)

```powershell
dotnet publish .\NaptimeDaemon\NaptimeDaemon.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Publish output:

- `NaptimeDaemon\bin\Release\net10.0-windows\win-x64\publish\`

## Notes

- App icon is embedded as a WPF resource for reliable runtime loading.
- EXE icon metadata is set via `ApplicationIcon` in `NaptimeDaemon.App.csproj`.
