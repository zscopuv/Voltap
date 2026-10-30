# Voltap

A lightweight Windows autoclicker built with **C# and WinForms**. Voltap supports multiple independent clicker instances, mouse buttons, keystroke output, configurable activation keys, Toggle/Hold modes, and per-instance CPS randomization.

## Features

- Multiple clickers can run at the same time.
- Output: left mouse, right mouse, middle mouse, or a configurable keystroke.
- Activation: Toggle or Hold.
- Individually configurable start key for every instance.
- Fixed CPS or random CPS range.
- Hard configuration cap of **250 CPS per instance** (4 ms minimum requested interval).
- Global hotkey polling while the app is running.
- Automatic settings persistence in `%AppData%\Voltap\config.json`.
- No third-party NuGet dependencies.

> Windows scheduling and the target application affect real-world timing. The 250 CPS limit is a configured ceiling, not a guarantee of exact delivery cadence.

## Requirements

- Windows 10 or later.
- .NET 8 SDK to build from source.

## Build and run

Open a terminal in the repository root:

```powershell
dotnet run --project .\Voltap\Voltap.csproj
```

Create a self-contained single-file build:

```powershell
dotnet publish .\Voltap\Voltap.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The published application is placed under `Voltap\bin\Release\net8.0-windows\win-x64\publish\`.

## Usage

1. Launch Voltap.
2. Select **Add clicker** and configure its output, activation mode, start key, and rate.
3. Use the configured start key anywhere in Windows while Voltap is running.
4. In **Toggle** mode, press the key once to start and again to stop. In **Hold** mode, the clicker runs only while the key is held.
5. Use **Start / Stop** to control the selected instance from the UI, or **Stop all** to stop every instance.

For keystroke output, choose the key to send in the clicker editor. The start key and output key are configured independently.

## Project layout

- `Voltap/Core` — input dispatch and click scheduling.
- `Voltap/Models` — clicker configuration models.
- `Voltap/Services` — configuration persistence.
- `Voltap/UI` — WinForms interface and clicker editor.

## Credits

Created by **zscopuv**.

- [GitHub repository](https://github.com/zscopuv/Voltap)
- [Voltap Source-Available License (VSAL)](https://raw.githubusercontent.com/zscopuv/Voltap/refs/heads/main/LICENSE)

See [LICENSE](LICENSE) for the complete terms.
